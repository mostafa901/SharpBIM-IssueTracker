using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitIssueComments : GitClient
    {
        public GitIssueComments()
        {
        }

        // References: https://docs.github.com/en/rest/issues/comments?apiVersion=2022-11-28#list-issue-comments
        protected override string endPoint => $"https://api.github.com/repos/{Owner}/REPO/issues/ISSUE_NUMBER/comments";

        protected override string GetEndPoint(params object[] values)
        {
            var url = base.GetEndPoint(values[0]);
            url = url.Replace("ISSUE_NUMBER", values[1].ToString());
            return url;
        }

        public async Task<IServiceReport<IEnumerable<CommentModel>>> GetCommentsForIssue(string repoName, int issueNumber)
        {
            var url = GetEndPoint(repoName, issueNumber);
            var reposne = await GET(url);
            var commentReport = new ServiceReport<IEnumerable<CommentModel>>();
            if (reposne.IsFailed)
            {
                commentReport.Merge(reposne);
            }
            else
            {
                commentReport.Model = ParseResponse<CommentModel>(reposne.Model);
            }
            return commentReport;
        }

        public async Task<IServiceReport<CommentModel>> PushComment(string repoName, int issueNumber, CommentModel comment)
        {
            //  https://api.github.com/repos/OWNER/REPO/issues/ISSUE_NUMBER/comments \
            var url = GetEndPoint(repoName, issueNumber);
            var body = new
            {
                comment.body
            };
            var pusheReport = new ServiceReport<CommentModel>();
            var response = await POST(url, body);
            if (response.IsFailed)
            {
                pusheReport.Merge(response);
            }
            else
            {
                pusheReport.Model = ParseResponse<CommentModel>(response.Model).FirstOrDefault();
            }

            return pusheReport;
        }
    }
}