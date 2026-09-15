using Digimezzo.Foundation.Core.Logging;
using Dopamine.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Dopamine.Core.Api.Lyrics
{
    public class LyricsFactory
    {
        private readonly IList<ILyricsApi> lyricsApis;
        private static readonly Random random = new Random();

        public LyricsFactory(int timeoutSeconds, string providers, ILocalizationInfo info)
        {
            lyricsApis = new List<ILyricsApi>();

            if (providers.ToLower().Contains("chartlyrics")) lyricsApis.Add(new ChartLyricsApi(timeoutSeconds));
            if (providers.ToLower().Contains("lololyrics")) lyricsApis.Add(new LololyricsApi(timeoutSeconds));
            if (providers.ToLower().Contains("neteaselyrics")) lyricsApis.Add(new NeteaseLyricsApi(timeoutSeconds, info));
        }

        public async Task<Lyrics> GetLyricsAsync(string artist, string title)
        {
            Lyrics lyrics = null;
            
            // Create a thread-safe local copy of the APIs and shuffle them
            var availableApis = lyricsApis.OrderBy(x => random.Next()).ToList();

            foreach (var api in availableApis)
            {
                try
                {
                    lyrics = new Lyrics(await api.GetLyricsAsync(artist, title), api.SourceName);
                    if (lyrics != null && lyrics.HasText)
                    {
                        break; // Found lyrics, stop searching
                    }
                }
                catch (Exception ex)
                {
                    LogClient.Error("Error while getting lyrics from '{0}'. Exception: {1}", api.SourceName, ex.Message);
                }
            }

            return lyrics;
        }
    }
}
