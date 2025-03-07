using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitLabels : GitClient
    {
        protected override string endPoint => @$"https://api.github.com/repos/{Owner}/REPO/labels";

        public async Task<IServiceReport<IEnumerable<GitLabel>>> GetLables(string repoName)
        {
            var url = GetEndPoint(repoName);
            var getReport = await GET(url);
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>(getReport);

            if (!getReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }

        public async Task<IServiceReport<IEnumerable<GitLabel>>> CreateLabel(string repoName, GitLabel newLable)
        {
            var url = GetEndPoint(repoName);
            var body = new
            {
                newLable.name,
                newLable.description,
                newLable.color,
            };
            var getReport = await POST(url, body);
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>(getReport);

            if (!getReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }

        public async Task<IServiceReport<IEnumerable<GitLabel>>> UpdateLabel(string repoName, GitLabel newLable)
        {
            var url = $"{GetEndPoint(repoName)}/{newLable.name}";
            var body = new
            {
                newLable.name,
                newLable.description,
                newLable.color,
            };
            var getReport = await PATCH(url, body);
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>(getReport);

            if (!getReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }
    }
}