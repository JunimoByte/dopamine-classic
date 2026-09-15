# Dopamine (Reverb) - Master Knowledge Base

Welcome to the comprehensive technical study and knowledge base for the **Dopamine** (Reverb) music player application. This directory contains deep-dive documentation analyzing the codebase from top to bottom.

---

## 🗺️ Knowledge Base Sitemap & Deep Dive Modules

| Module | Topic | Key Systems & Files Covered |
| :--- | :--- | :--- |
| **[01. Architecture & Lifecycle](./01_ARCHITECTURE_AND_LIFECYCLE.md)** | Core Architecture, Startup & DI | `App.xaml.cs`, Prism DryIoc DI, Single Instance Mutex, CLI processing, WCF Named Pipe IPC, AppDomain exception handling. |
| **[02. Audio Engine & DSP](./02_AUDIO_ENGINE_AND_DSP.md)** | CSCore Audio Pipeline | `CSCorePlayer.cs`, WASAPI vs DirectSound, FFmpeg/MediaFoundation decoders, 10-Band BiQuad EQ, Real-time FFT notification stream. |
| **[03. Playback & Queue](./03_PLAYBACK_AND_QUEUE_MANAGEMENT.md)** | Queue & State Machine | `PlaybackService.cs`, `QueueManager.cs`, Index Indirection shuffling, Loop modes, Track statistics (Play/Skip count), Queue persistence. |
| **[04. Database & Data Layer](./04_DATABASE_AND_DATA_LAYER.md)** | SQLite Schema & Repositories | `SQLiteConnectionFactory.cs`, `DbMigrator.cs` (v1-v27), `SafePath` indexing pattern, Entity models (`Track`, `Folder`, `AlbumArtwork`, etc.). |
| **[05. Indexing & Metadata](./05_INDEXING_METADATA_AND_CACHE.md)** | File Scanning & Cover Art | `IndexingService.cs`, `FolderWatcherManager`, TagLib metadata extraction, ID3 multi-value tag patcher, `AlbumKey`, Artwork hashing & disk cache. |
| **[06. UI Architecture & Modes](./06_UI_MVVM_AND_MODES.md)** | WPF, MVVM & 4 Player Views | `Shell.xaml`, `FullPlayer`, `CoverPlayer`, `MicroPlayer`, `NanoPlayer`, `SlidingContentControl`, Now Playing views, Dynamic album cover color extraction. |
| **[07. External Integrations](./07_EXTERNAL_INTEGRATIONS_AND_SERVICES.md)** | OS & Online Integrations | WCF IPC & Rainmeter FFT API, Windows 10 SMTC, Toast & Legacy Notifications, Last.fm Scrobbler, Multi-source Lyrics scrapers, Discord RPC. |
| **[08. Tray Controls & Track Transitions](./08_TRAY_CONTROLS_AND_TRACK_TRANSITIONS.md)** | System Tray Popup & Progression Engine | `TrayControls.xaml/.cs`, Taskbar docking geometry, Aero blur, 3-Second rule on Back, Manual vs Auto Next decision tree, LoopOne overrides, Blacklist filtering. |

---

## ⚡ Quick Architectural Summary

```
                      +---------------------------------------+
                      |         Dopamine (WPF UI Layer)       |
                      |  - Shell Window (4 Player Modes)      |
                      |  - TrayControls (System Tray Popup)   |
                      |  - Prism MVVM & Region Navigation     |
                      |  - Custom Controls & Color Theming    |
                      +-------------------+-------------------+
                                          |
                      +-------------------+-------------------+
                      |           Dopamine.Services           |
                      |  - PlaybackService & QueueManager     |
                      |  - IndexingService & Watcher          |
                      |  - Appearance, Lyrics, Scrobbling     |
                      |  - ExternalControl IPC & Discord RPC  |
                      +---------+-------------------+---------+
                                |                   |
            +-------------------+----+         +----+-------------------+
            |     Dopamine.Data      |         |     Dopamine.Core      |
            |  - SQLite DB & Migrator|         |  - CSCore Audio Engine |
            |  - Track & Folder Repos|         |  - Web APIs & Helpers  |
            |  - Metadata Extractors |         |  - Constants & Enums   |
            +------------------------+         +------------------------+
```

### Key Technical Concepts to Know

1. **System Tray Preview (`TrayControls`)**: Custom borderless WPF popup positioned according to the Windows Taskbar orientation (Top/Bottom/Left/Right), with acrylic blur and automatic dismissal on outside clicks (`Window_Deactivated`).
2. **Transition Decision Engine**:
   - **Back**: Re-plays current song if $> 3\text{ seconds}$ elapsed; otherwise moves to previous index in `playbackOrder` (wrapping to end if `LoopMode.All`).
   - **Next**: Manual click breaks out of `LoopMode.One`; skips consecutive duplicate paths; wraps or stops based on `LoopMode.All` and `LoopWhenShuffle`.
   - **Auto Next**: Increments play count on finished track and silently skips over blacklisted songs with loop-guard protection.
3. **Prism Modular DI**: Uses DryIoc container registered in `App.xaml.cs`. View navigation uses Prism RegionManager with animated slide transitions.
4. **Audio Graph**: Audio stream $\to$ Codec (FFmpeg/MF) $\to$ Resampler $\to$ 10-Band BiQuad EQ $\to$ `SingleBlockNotificationStream` (FFT Tap) $\to$ `WasapiOut`.
5. **Queue Index Indirection**: Shuffling re-indexes an array of pointers (`playbackOrder`) rather than rearranging track objects, guaranteeing the currently playing track stays uninterrupted.
