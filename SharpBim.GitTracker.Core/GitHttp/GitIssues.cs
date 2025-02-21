using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.GitTracker.GitHttp
{
    public class GitIssues : GitClient
    {
        // References: https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#create-an-issue
        protected override string endPoint => $"https://api.github.com/repos/{Account.login}/REPO/issues";

        private string GetEndPoint(string repoName) => endPoint.Replace("REPO", repoName);

        internal GitIssues()
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
            //  request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.TXTJSON)); //textOnly
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.FULLJSON)); //txt, html, markdown
        }

        public async Task<IEnumerable<IssueModel>> GetAllIssues(RepoModel repo, IssueState state)
        {
            return await GetIssues(repo.name, -1, state);
        }

        public async Task<IssueModel> GetIssue(RepoModel repo, int number)
        {
            var issue = (await GetIssues(repo.name, number, IssueState.open))?.FirstOrDefault();
            return issue;
        }

        public async Task<IEnumerable<IssueModel>> GetIssues(string repoName, int number, IssueState state)
        {
            //  https://api.github.com/repos/OWNER/REPO/issues/ISSUE_NUMBER
            var url = $"{GetEndPoint(repoName)}";
            if (number > 0)
            {
                url += $"/{number}";
            }
            url = $"{url}?state={state}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await GET(request);
            var issues = ParseResponse<IssueModel>(response);
            return issues;
        }
    }
}