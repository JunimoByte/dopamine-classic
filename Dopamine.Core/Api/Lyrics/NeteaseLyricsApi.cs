using Digimezzo.Foundation.Core.Settings;
using Dopamine.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Dopamine.Core.Api.Lyrics
{
    // API from http://moonlib.com/606.html
    public class NeteaseLyricsApi : ILyricsApi
    {
        private ILocalizationInfo info;
        private const string apiSearchResultLimit = "1";

        private const string apiLyricsFormat = "song/lyric?os=pc&id={0}&lv=-1&tv=-1";

        private const string apiRootUrl = "http://music.163.com/api/";
        private int timeoutSeconds;
        private static readonly HttpClient httpClient;
        private bool enableTLyric;

        static NeteaseLyricsApi()
        {
            httpClient = new HttpClient(new HttpClientHandler() {AutomaticDecompression = DecompressionMethods.GZip})
            {
                BaseAddress = new Uri(apiRootUrl)
            };
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,zh-CN;q=0.8,zh;q=0.6,en;q=0.7");
            httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.186 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Referer", "http://music.163.com/");
            httpClient.DefaultRequestHeaders.Add("Host", "music.163.com");
            httpClient.DefaultRequestHeaders.Add("Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
        }

        public NeteaseLyricsApi(int timeoutSeconds, ILocalizationInfo info)
        {
            this.timeoutSeconds = timeoutSeconds;
            this.info = info;
            this.enableTLyric = SettingsClient.Get<string>("Appearance", "Language") == "ZH-CN";
        }

        private async Task<string> ParseTrackIdAsync(string artist, string title)
        {
            var postContent = new Dictionary<string, string>
            {
                {"s", title + " " + artist},
                {"limit", apiSearchResultLimit},
                {"type", "1"},
                {"offset", "0"}
            };

            var response =
                await (await httpClient.PostAsync("search/pc", new FormUrlEncodedContent(postContent))).Content
                    .ReadAsStringAsync();

            int start = response.IndexOf("\"id\":") + 6;
            if (start < 6) return string.Empty;
            int end = response.IndexOf(",\"position\":", start);
            if (end < 0) return string.Empty;
            return response.Substring(start, end - start);
        }

        private async Task<string> ParseLyricsAsync(string trackId)
        {
            string resJson = await httpClient.GetStringAsync(String.Format(apiLyricsFormat, trackId));
            
            var jss = new JavaScriptSerializer();
            var dict = jss.Deserialize<Dictionary<string, object>>(resJson);

            string lrcLyric = string.Empty;
            string tLyric = string.Empty;

            if (dict != null)
            {
                if (dict.ContainsKey("lrc"))
                {
                    var lrcDict = dict["lrc"] as Dictionary<string, object>;
                    if (lrcDict != null && lrcDict.ContainsKey("lyric"))
                    {
                        lrcLyric = lrcDict["lyric"]?.ToString();
                    }
                }
                
                if (this.enableTLyric && dict.ContainsKey("tlyric"))
                {
                    var tlrcDict = dict["tlyric"] as Dictionary<string, object>;
                    if (tlrcDict != null && tlrcDict.ContainsKey("lyric"))
                    {
                        tLyric = tlrcDict["lyric"]?.ToString();
                    }
                }
            }
            
            if (string.IsNullOrEmpty(tLyric))
                return lrcLyric;
                
            return lrcLyric + "\r\n" + tLyric;
        }

        public async Task<string> GetLyricsAsync(string artist, string title)
        {
            string trackId = await ParseTrackIdAsync(artist, title);

            if (!string.IsNullOrEmpty(trackId))
            {
                return await ParseLyricsAsync(trackId);
            }

            return string.Empty;
        }

        public string SourceName => "NeteaseMusic";
    }
}
