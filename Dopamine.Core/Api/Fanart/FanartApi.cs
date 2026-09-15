using Dopamine.Core.Base;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Dopamine.Core.Api.Fanart
{
    public static class FanartApi
    {
        private const string apiRootFormat = "https://webservice.fanart.tv/v3/music/{0}?api_key={1}";
        private static readonly HttpClient httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        public async static Task<string> GetArtistThumbnailAsync(string musicBrainzId)
        {
            try
            {
                string jsonResult = await GetArtistImages(musicBrainzId);
                var jss = new JavaScriptSerializer();
                var dict = jss.Deserialize<Dictionary<string, object>>(jsonResult);

                if (dict != null && dict.ContainsKey("artistthumb"))
                {
                    var thumbs = dict["artistthumb"] as System.Collections.ArrayList;
                    if (thumbs != null && thumbs.Count > 0)
                    {
                        var firstThumb = thumbs[0] as Dictionary<string, object>;
                        if (firstThumb != null && firstThumb.ContainsKey("url"))
                        {
                            return firstThumb["url"].ToString();
                        }
                    }
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private async static Task<string> GetArtistImages(string musicBrainzId)
        {
            string result = string.Empty;

            Uri uri = new Uri(string.Format(apiRootFormat, musicBrainzId, SensitiveInformation.FanartApiKey));

            try
            {
                var response = await httpClient.GetAsync(uri);
                result = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                // Ignored
            }

            return result;
        }
    }
}
