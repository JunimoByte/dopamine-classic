# 05. Indexing, Metadata & Artwork Caching

## 1. Library Indexing Architecture

The indexing pipeline monitors user-configured music directories, extracts tag metadata and embedded artwork, computes album groupings, and synchronizes the SQLite database.

```mermaid
sequenceDiagram
    autonumber
    participant Watcher as FolderWatcherManager
    participant Indexer as IndexingService
    participant Parser as FileMetadata / TagLib
    participant Cache as CacheService (Disk Cache)
    participant DB as SQLite DB

    Watcher->>Indexer: Directory Changed / Startup Scan Trigger
    Indexer->>DB: Query existing tracked files (Path, DateFileModified)
    Indexer->>Indexer: Diff Disk Files vs DB Files
    
    par Phase 1: Track Removal
        Indexer->>DB: Delete tracks removed from disk
    and Phase 2: Metadata Extraction
        loop For Each New or Modified Track
            Indexer->>Parser: Read ID3 / Vorbis / MP4 tags
            Parser->>Indexer: FileMetadata (Artist, Album, Title, Year, Multi-value tags)
            Indexer->>DB: Insert / Update Track Record
        end
    and Phase 3: Album Artwork Extraction
        loop For Each AlbumKey needing artwork
            Indexer->>Cache: Extract embedded TagLib picture or folder image (cover.jpg/folder.jpg)
            Cache->>Cache: Hash artwork bytes -> Generate unique ArtworkID
            Cache->>Cache: Save scaled thumbnail to disk cache (%AppData%\Dopamine\Cache)
            Indexer->>DB: Map AlbumKey -> ArtworkID in AlbumArtwork table
        end
    end

    Indexer->>Indexer: Fire RefreshLists / RefreshArtwork Events
```

---

## 2. Filesystem Monitoring (`FolderWatcherManager`)

Dopamine avoids hammering the disk with `GentleFolderWatcher`:
- Uses .NET `FileSystemWatcher` wrapped with event throttling/debouncing.
- Filters events (`Created`, `Changed`, `Deleted`, `Renamed`).
- Ignores temporary files, playlist files, and non-supported extensions.
- When disk changes settle, raises `FoldersChanged` to trigger incremental indexing.

---

## 3. Metadata Extraction & ID3 Tag Cleaning

Located in `Dopamine.Data.MetaDataUtils.cs`:

### 1. Multi-Value Tag Delimiting
- Artists and Genres often contain multiple values (e.g. `["Queen", "David Bowie"]`).
- In ID3v2.3, slash `/` was historically used as a separator, which corrupted artists like `AC/DC`.
- `MetadataUtils.PatchID3v23Enumeration()` joins known unsplittable tokens (`Defaults.UnsplittableTagValues`) before splitting and reformats them using Dopamine's internal multi-value delimiter format: `; ` or bracket-delimited tokens.

### 2. Album Key Generation (`AlbumKey`)
To correctly group tracks into cohesive albums (preventing split albums across multi-disc or varied artist tracks):
- Algorithm: `GenerateInitialAlbumKey(AlbumTitle, AlbumArtists)`
- Evaluates:
  1. Album Title (normalized, whitespace-trimmed).
  2. Album Artist (`AlbumArtists` tag). If empty, falls back to `Artist` or flags as `Various Artists` for compilations.
  3. Year (if configured to split same-titled albums).

---

## 4. Artwork Extraction, Caching & Hashing

Located in `CacheService.cs` (`Dopamine.Services.Cache`):

### Artwork Resolution Hierarchy
When indexing artwork for a track/album:
1. **Embedded TagLib Artwork**: Scans embedded front covers in audio file tags (`IPicture`).
2. **Folder Cover Images**: If no embedded art exists, scans the audio file's directory for known artwork names:
   - `cover.jpg`, `cover.png`
   - `folder.jpg`, `folder.png`
   - `albumart.jpg`, `front.jpg`
3. **Online Download Fallback**: If configured, `InfoDownloadService` queries Last.fm / Fanart.tv APIs for missing album art.

### Artwork Deduplication & Caching
- Raw image bytes are hashed (MD5 / SHA) to create an **`ArtworkID`**.
- Duplicate album artworks across different albums share the exact same cached image file on disk.
- Cache location: `%AppData%\Dopamine\Cache\Artwork\{ArtworkID}.jpg`.
- Images are pre-scaled and optimized for fast WPF asynchronous bitmap loading (`BitmapImage` with `DecodePixelWidth` to minimize RAM).
