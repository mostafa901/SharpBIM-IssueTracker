

using Microsoft;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitIssues : GitClient
    {
        // References: https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#create-an-issue
        // protected override string EndPoint => $"https://api.github.com/repos/{Owner}/REPO/issues";

        protected override string GetEndPoint(params object[] values)
        {
            var vs = values.ToList();
            vs.Insert(1, "issues");
            var url = base.GetEndPoint(vs.ToArray());
            return url;
        }
        public GitIssues(IConfig appGlobals) : base(appGlobals)
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
            //  request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.TXTJSON)); //textOnly
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.FULLJSON)); //txt, html, markdown
        }

#if false
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
#endif

        public async Task<IServiceReport<IssueModel>> GetIssue(RepoModel repoModel, int number)
        {
            var getissueReport = await GetIssues(repoModel, number, IssueState.open);
            var issueReport = new ServiceReport<IssueModel>();
            issueReport.Merge(getissueReport);
            if (!issueReport.IsFailed)
            {
                var issue = (await GetIssues(repoModel, number, IssueState.open)).Model.FirstOrDefault();
                issueReport.Model = issue;
            }
            return issueReport;
        }

        public async Task<IServiceReport<IEnumerable<IssueModel>>> GetSubIssues(RepoModel repoModel, int number, int page = 1)
        {
            var url = GetEndPoint(repoModel, number, "sub_issues");

            var bodyParams = new
            {
                page,
                per_page = 100,
            };
            url = EmbedParams(url, bodyParams);

            var getissueReport = await GET(url);
            var issueReport = new ServiceReport<IEnumerable<IssueModel>>();
            if (!getissueReport.IsFailed)
            {
                var issues = ParseResponse<IssueModel>(getissueReport.Model);
                issueReport.Model = issues.Where(o => o.pull_request == null);
            }
            return issueReport;
        }


        public async Task<IServiceReport<IEnumerable<IssueModel>>> GetIssues(RepoModel repoModel, int number, IssueState state, int page = 1)
        {
            //  https://api.github.com/repos/OWNER/REPO/issues/ISSUE_NUMBER
            var url = GetEndPoint(repoModel);
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
            var issueReport = new ServiceReport<IEnumerable<IssueModel>>();
            issueReport.Merge(report);
            if (!issueReport.IsFailed)
            {
                var response = report.Model;
                var issues = ParseResponse<IssueModel>(response);
                issueReport.Model = issues.Where(o => o?.pull_request == null);
            }
            return issueReport;
        }

         
        //https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#update-an-issue
        // You cannot pass both `assignee` and `assignees`. Only one may be provided.
        public async Task<IServiceReport<IssueModel>> CreateIssue(RepoModel repoModel, IssueModel issue)
        {
            var url = $"{GetEndPoint(repoModel)}";


            // Set the content type to JSON
            //  var content = new StringContent(issue.JSerialize(), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
            var paybody = new
            {
                issue.body,
                title = issue.Title,
                //assignee, // or Assignees or null
                issue.state,
                //milestone ,
                labels = issue.labels?.Select(o => o.Name).ToArray() ?? [],
                //state_reason , // Can be one of: completed, not_planned, reopened, null
            };
            IServiceReport<string> response = null;

            response = await POST(url, paybody);
            var report = new ServiceReport<IssueModel>();
            report.Merge(response);
            if (!report.IsFailed)
            {
                report.Model = ParseResponse<IssueModel>(response.Model)?.FirstOrDefault();
            }
            return report;
        }

        //https://docs.github.com/en/rest/issues/issues?apiVersion=2022-11-28#update-an-issue
        // You cannot pass both `assignee` and `assignees`. Only one may be provided.
        public async Task<IServiceReport<IssueModel>> PatchIssue(RepoModel repoModel, IssueModel issue)
        {
            var url = $"{GetEndPoint(repoModel, issue.number)}";

            // Set the content type to JSON
            //  var content = new StringContent(issue.JSerialize(), Encoding.UTF8, MediaTypes.VNDGITHUBJSON);
            var paybody = new
            {
                issue.body,
                title = issue.Title,
                assignees = issue.assignees?.Select(o => o.login).ToArray(),
                issue.state,
                //milestone ,
                labels = issue.labels?.Select(o => o.Name).ToArray() ?? [],
                //state_reason , // Can be one of: completed, not_planned, reopened, null
            };
            IServiceReport<string> response = null;

            response = await PATCH(url, paybody);
            var report = new ServiceReport<IssueModel>();
            report.Merge(response);
            if (!report.IsFailed)
            {
                var patchedIssueReport = ParseResponse<IssueModel>(response.Model)?.FirstOrDefault();
                report.Model = patchedIssueReport;
            }
            return report;
        }

        // this requires contentsPErmisison Read and write
        public async Task<IServiceReport<string>> UploadImageAsync(RepoModel repoModel, string filePath, int issueNumber, string branch = "master")
        {
            string imageName = Path.GetFileName(filePath);
            //string url = $"https://api.github.com/repos/{Owner}/{repoName}/contents/issue-images/{issueNumber}/{imageName}";
            string url = GetEndPoint(repoModel, "contents", "issue-images", issueNumber, imageName);

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

        public async Task<IServiceReport<IssueModel>> AddSubIssue(RepoModel repoModel, IssueModel parent, IssueModel subIssue, bool forceChange)
        {
            var url = GetEndPoint(repoModel, parent.number, "sub_issues");

            var body = new
            {
                sub_issue_id = subIssue.Id,
                replace_parent = forceChange
            };

            IServiceReport<string> report = await POST(url, body);
            var issueReport = new ServiceReport<IssueModel>();
            if (!report.IsFailed)
            {
                issueReport.Model = ParseResponse<IssueModel>(report.Model).FirstOrDefault();
            }
            return issueReport;
        }
    }
}