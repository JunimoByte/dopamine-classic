using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace Dopamine.Core.Api.GitHub
{
    public static class GitHubApi
    {
        private static readonly HttpClient httpClient = new HttpClient();
        
        public static async Task<string> GetLatestReleaseAsync(string owner, string repo, bool includePrereleases) {
            var url = $"https://api.github.com/repos/{owner}/{repo}/releases";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

            var releasesResponse = await httpClient.GetStringAsync(url);
            
            var jss = new JavaScriptSerializer();
            var releases = jss.Deserialize<List<Dictionary<string, object>>>(releasesResponse);

            Dictionary<string, object> latestRelease = null;

            if (includePrereleases)
            {
                latestRelease = releases.FirstOrDefault(x => x.ContainsKey("prerelease") && (bool)x["prerelease"]);
            }
            else
            {
                latestRelease = releases.FirstOrDefault(x => x.ContainsKey("prerelease") && !(bool)x["prerelease"]);
            }

            if (latestRelease != null && latestRelease.ContainsKey("tag_name") && latestRelease["tag_name"] != null)
            {
                return latestRelease["tag_name"].ToString().Replace("v", "");
            }

            return string.Empty;
        }
    }
}

