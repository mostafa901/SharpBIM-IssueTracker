using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using Microsoft.Web.WebView2.Core;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.GitTracker.Core.JsonConverters;
using SharpBIM.ServiceContracts.Interfaces;
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
            var report = await GET(request);
            if (!report.IsFailed)
            {
                var response = report.Model;
                var issues = ParseResponse<IssueModel>(response);
                return issues;
            }
            return null;
        }

        protected override JsonSerializerOptions GetPostOptions()
        {
            var js = base.GetPostOptions();

            js.Converters.Add(new IssueJsonConv());
            return js;
        }

        //https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#update-an-issue
        // You cannot pass both `assignee` and `assignees`. Only one may be provided.
        public async Task<bool> PushIssue(IssueModel issue)
        {
            var url = $"{issue.url}";

            // Set the content type to JSON
            //  var content = new StringContent(issue.JSerialize(), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
            var paybody = new
            {
                issue.body,
                issue.Title
            };
            var response = await PATCH(url, paybody);

            return true;
        }

        // this requires contentsPErmisison Read and write
        public async Task<IServiceReport<string>> UploadImageAsync(RepoModel repo, string filePath, string branch = "master")
        {
            string imageName = Path.GetFileName(filePath);
            string url = $"https://api.github.com/repos/{repo.owner.login}/{repo.name}/contents/images/{imageName}";

            // Convert image to Base64
            byte[] imageBytes = File.ReadAllBytes(filePath);
            string base64Image = Convert.ToBase64String(imageBytes);

            var payload = new
            {
                message = "Uploading an image for issue",
                content = base64Image,
                branch = branch
            };

            var report = await PUT(url, payload);
            if (!report.IsFailed)
            {
                using JsonDocument doc = JsonDocument.Parse(report.Model);
                if (doc.RootElement.TryGetProperty("content", out var content))
                {
                    report.Model = content.GetProperty("html_url").GetString();  // Get the image URL
                }
            }
            return report;
        }
    }
}