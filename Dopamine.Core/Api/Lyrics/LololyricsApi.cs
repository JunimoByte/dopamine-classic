using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dopamine.Core.Api.Lyrics
{
    public class LololyricsApi : ILyricsApi
    {
        private const string apiRootFormat = "https://api.lololyrics.com/0.5/getLyric?artist={0}&track={1}";
        private int timeoutSeconds;
        private static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        public LololyricsApi(int timeoutSeconds)
        {
            this.timeoutSeconds = timeoutSeconds;
        }

        public async Task<string> ParseResultAsync(string result)
        {
            string lyrics = string.Empty;

            await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // https://api.lololyrics.com/
                    var resultXml = XDocument.Parse(result);

                    // Status
                    string status = (from t in resultXml.Element("result").Elements("status")
                                     select t.Value).FirstOrDefault();

                    if (status != null && status.ToLower() == "ok")
                    {
                        lyrics = (from t in resultXml.Element("result").Elements("response")
                                  select t.Value).FirstOrDefault();
                    }
                }
            });

            return lyrics;
        }

        public string SourceName
        {
            get
            {
                return "LoloLyrics";
            }
        }

        /// <summary>
        /// Searches for lyrics for the given artist and title
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            Uri uri = new Uri(string.Format(apiRootFormat, artist, title));

            string result = string.Empty;

            try
            {
                var response = await httpClient.GetAsync(uri);
                result = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                // Ignore exceptions like timeouts or network errors
            }

            string lyrics = await ParseResultAsync(result);

            return lyrics;
        }
    }
}