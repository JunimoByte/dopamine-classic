using Dopamine.Core.Api.Lastfm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dopamine.Services.InfoDownload
{
    public class InfoDownloadService : IInfoDownloadService
    {
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<string> GetAlbumImageAsync(string albumTitle, IList<string> albumArtists, string trackTitle = "", IList<string> trackArtists = null)
        {
            string title = string.Empty;
            List<string> artists = new List<string>();

            // Title
            if (!string.IsNullOrEmpty(albumTitle))
            {
                title = albumTitle;
            }
            else if (!string.IsNullOrEmpty(trackTitle))
            {
                title = trackTitle;
            }

            // Artist
            if (albumArtists != null && albumArtists.Count > 0)
            {
                artists.AddRange(albumArtists.Where(a => !string.IsNullOrEmpty(a)));
            }

            if (trackArtists != null && trackArtists.Count > 0)
            {
                artists.AddRange(trackArtists.Where(a => !string.IsNullOrEmpty(a)));
            }

            if (string.IsNullOrEmpty(title) || artists == null || artists.Count == 0)
            {
                return null;
            }

            foreach (string artist in artists)
            {
                // 1. Try iTunes API first (huge database, high quality)
                try
                {
                    string query = Uri.EscapeDataString($"{artist} {title}");
                    string itunesUrl = $"https://itunes.apple.com/search?term={query}&entity=album&limit=1";
                    
                    using (var response = await httpClient.GetAsync(itunesUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            // Simple regex extraction to avoid pulling in full JSON parsing overhead
                            Match match = Regex.Match(json, @"""artworkUrl100"":\s*""([^""]+)""");
                            if (match.Success)
                            {
                                string artworkUrl = match.Groups[1].Value;
                                // iTunes returns 100x100 thumbnails by default. Replace with 600x600 for high quality.
                                artworkUrl = artworkUrl.Replace("100x100bb", "600x600bb");
                                return artworkUrl;
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore iTunes errors and fall back to Last.fm
                }

                // 2. Fallback to Last.fm API
                try
                {
                    LastFmAlbum lfmAlbum = await LastfmApi.AlbumGetInfo(artist, title, false, "EN");
                    if (lfmAlbum != null && !string.IsNullOrEmpty(lfmAlbum.LargestImage()))
                    {
                        return lfmAlbum.LargestImage();
                    }
                }
                catch
                {
                    // Ignore Last.fm errors
                }
            }

            return null;
        }
    }
}
