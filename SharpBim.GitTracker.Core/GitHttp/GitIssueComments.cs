using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitIssueComments : GitClient
    {
        public GitIssueComments(IConfig appGlobals): base(appGlobals)
        {
        }

        // References: https://docs.github.com/en/rest/issues/comments?apiVersion=2022-11-28#list-issue-comments
        protected override string EndPoint => $"https://api.github.com/repos/{Owner}/REPO/issues/ISSUE_NUMBER/comments";

        protected override string GetEndPoint(params object[] values)
        {
            RepoModel repoModel = values[0] as RepoModel;
            var issueNumber = (int)values[1];
            var url = $"{repoModel.url}/issues/{issueNumber}/comments";
            return url;
        }

        public async Task<IServiceReport<IEnumerable<CommentModel>>> GetCommentsForIssue(RepoModel repoModel, int issueNumber)
        {
            var url = GetEndPoint(repoModel, issueNumber);
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

        public async Task<IServiceReport<CommentModel>> PushComment(RepoModel repoModel, int issueNumber, CommentModel comment)
        {
            //  https://api.github.com/repos/OWNER/REPO/issues/ISSUE_NUMBER/comments \
            var url = GetEndPoint(repoModel, issueNumber);
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