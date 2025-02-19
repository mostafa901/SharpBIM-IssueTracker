using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using SharpBim.GitTracker.Core.Enums;
using SharpBIM.Utility.Extensions;

namespace SharpBim.GitTracker.GitHttp
{
    public class GitIssues : GitClient
    {
        protected override string endPoint => "https://api.github.com/repos/OWNER/REPO/issues";

        public GitIssues()
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.TXTJSON));
        }

        public async Task<IEnumerable<IssueModel>> GetAllIssues(RepoModel repo, IssueState state)
        {
            var url = repo.issues_url.Replace(NUMBER, "");

            url = $"{url}?state={string.Join(",", state.GetFlags().Select(o => o.ToString()))}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await GET(request);
            var issues = ParseResponse<IssueModel>(response);
            return issues;
        }

        public async Task<IssueModel> GetIssue(RepoModel repo, int number)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, repo.issues_url.Replace(NUMBER, "/" + number.ToString()));
            var response = await GET(request);
            var issue = ParseResponse<IssueModel>(response)?.FirstOrDefault();
            return issue;
        }

        protected override IEnumerable<T> ParseResponse<T>(string response)
        {
            if (response == null)
                return null;
            List<T> result = new List<T>();
            if (response.StartsWith("["))
                result.AddRange(JsonSerializer.Deserialize<IEnumerable<T>>(response));
            else
                result.Add(JsonSerializer.Deserialize<T>(response));

            if (result.Any() == false)
                return null;
            return result;
        }
    }
}