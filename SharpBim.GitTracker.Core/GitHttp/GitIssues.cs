using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Web.WebView2.Core;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.GitTracker.Core.JsonConverters;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitIssues : GitClient
    {
        // References: https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#create-an-issue
        protected override string endPoint => $"https://api.github.com/repos/{Owner}/REPO/issues";

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

        public async Task<IServiceReport<IEnumerable<IssueModel>>> GetAllIssues(string repoName, IssueState state)
        {
            int pages = 1;
            var report = new ServiceReport<IEnumerable<IssueModel>>();
            while (true)
            {
                var issueReport = await GetIssues(repoName, -1, state, pages++);
                if (issueReport.IsFailed)
                {
                    report.Merge(issueReport);
                    break;
                }
                if (issueReport.Model.Any())
                {
                    report.Model = report.Model.Concat(issueReport.Model);

                    continue;
                }
                break;
            }

            return report;
        }

        public async Task<IServiceReport<IssueModel>> GetIssue(string repoName, int number)
        {
            var getissueReport = await GetIssues(repoName, number, IssueState.open);
            var issueReport = new ServiceReport<IssueModel>(getissueReport);
            if (!issueReport.IsFailed)
            {
                var issue = (await GetIssues(repoName, number, IssueState.open)).Model.FirstOrDefault();
                issueReport.Model = issue;
            }
            return issueReport;
        }

        public async Task<IServiceReport<IEnumerable<IssueModel>>> GetSubIssues(string repoName, int number, int page = 1)
        {
            var url = $"{GetEndPoint(repoName)}/{number}/sub_issues";

            var bodyParams = new
            {
                page,
                per_page = 100,
            };
            url = EmbedParams(url, bodyParams);

            var getissueReport = await GET(url);
            var issueReport = new ServiceReport<IEnumerable<IssueModel>>(getissueReport);
            if (!issueReport.IsFailed)
            {
                issueReport.Model = ParseResponse<IssueModel>(getissueReport.Model);
            }
            return issueReport;
        }

        public async Task<IServiceReport<IEnumerable<IssueModel>>> GetIssues(string repoName, int number, IssueState state, int page = 1)
        {
            //  https://api.github.com/repos/OWNER/REPO/issues/ISSUE_NUMBER
            var url = $"{GetEndPoint(repoName)}";
            if (number > 0)
            {
                url += $"/{number}";
            }
            else
            {
                var bodyParams = new
                {
                    page,
                    //       milestone = "*",
                    state = state.ToString(),
                    //          assignee = "*", // for any user
                    //creator,
                    //mentioned ,
                    //labels  ,
                    //sort   ,
                    //direction    ,
                    //since     ,
                    per_page = 100,
                };
                url = EmbedParams(url, bodyParams);
            }

            var report = await GET(url);
            var issueReport = new ServiceReport<IEnumerable<IssueModel>>(report);
            if (!issueReport.IsFailed)
            {
                var response = report.Model;
                var issues = ParseResponse<IssueModel>(response);
                issueReport.Model = issues;
            }
            return issueReport;
        }

        protected override JsonSerializerOptions GetPostOptions()
        {
            var js = base.GetPostOptions();

            js.Converters.Add(new IssueJsonConv());
            return js;
        }

        //https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#update-an-issue
        // You cannot pass both `assignee` and `assignees`. Only one may be provided.
        public async Task<IServiceReport<IssueModel>> CreateIssue(string repoName, IssueModel issue)
        {
            var url = $"{GetEndPoint(repoName)}";

            // Set the content type to JSON
            //  var content = new StringContent(issue.JSerialize(), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
            var paybody = new
            {
                issue.body,
                title = issue.Title,
                //assignee, // or Assignees or null
                issue.state,
                //milestone ,
                labels = issue.labels?.Select(o => o.name).ToArray() ?? [],
                //state_reason , // Can be one of: completed, not_planned, reopened, null
            };
            IServiceReport<string> response = null;

            response = await POST(url, paybody);
            if (!response.IsFailed)
            {
                var createIssueReport = ParseResponse<IssueModel>(response.Model)?.FirstOrDefault();
                return new ServiceReport<IssueModel>(createIssueReport);
            }
            return new ServiceReport<IssueModel>().Failed(response.ErrorMessage);
        }

        //https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#update-an-issue
        // You cannot pass both `assignee` and `assignees`. Only one may be provided.
        public async Task<IServiceReport<IssueModel>> PatchIssue(string repoName, IssueModel issue)
        {
            var url = $"{GetEndPoint(repoName)}";
            url += $"/{issue.number}";

            // Set the content type to JSON
            //  var content = new StringContent(issue.JSerialize(), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
            var paybody = new
            {
                issue.body,
                title = issue.Title,
                //assignee, // or Assignees or null
                issue.state,
                //milestone ,
                labels = issue.labels?.Select(o => o.name).ToArray() ?? [],
                //state_reason , // Can be one of: completed, not_planned, reopened, null
            };
            IServiceReport<string> response = null;

            response = await PATCH(url, paybody);

            if (!response.IsFailed)
            {
                var patchedIssueReport = ParseResponse<IssueModel>(response.Model)?.FirstOrDefault();
                return new ServiceReport<IssueModel>(patchedIssueReport);
            }
            return new ServiceReport<IssueModel>().Failed(response.ErrorMessage);
        }

        // this requires contentsPErmisison Read and write
        public async Task<IServiceReport<string>> UploadImageAsync(string repoName, string filePath, int issueNumber, string branch = "master")
        {
            string imageName = Path.GetFileName(filePath);
            string url = $"https://api.github.com/repos/{Owner}/{repoName}/contents/issue-images/{issueNumber}/{imageName}";

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

        public async Task<IServiceReport<IssueModel>> AddSubIssue(string repoName, IssueModel parent, IssueModel subIssue, bool forceChange)
        {
            var url = $"{GetEndPoint(repoName)}/{parent.number}/sub_issues";

            var body = new
            {
                sub_issue_id = subIssue.Id,
                replace_parent = forceChange
            };

            IServiceReport<string> report = await POST(url, body);
            var issueReport = new ServiceReport<IssueModel>(report);
            if (!issueReport.IsFailed)
            {
                issueReport.Model = ParseResponse<IssueModel>(report.Model).FirstOrDefault();
            }
            return issueReport;
        }
    }
}