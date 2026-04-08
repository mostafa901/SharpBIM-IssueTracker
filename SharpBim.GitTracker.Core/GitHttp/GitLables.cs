using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitLabels : GitClient
    {
        public GitLabels(IConfig appGlobals) : base(appGlobals)
        {
        }
        const string LABELS = "labels";
        protected override string EndPoint => @$"https://api.github.com/repos/{Owner}/REPO/{LABELS}";

        public async Task<IServiceReport<IEnumerable<GitLabel>>> GetLables(RepoModel repoModel)
        {
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>();
            var url = $"{repoModel.url}/{LABELS}";
            var getReport = await GET(url);
            if(getReport.IsFailed)
            {
                labelReport.Merge(getReport);
                return labelReport;
            }
            labelReport.Merge(getReport);
            if (!labelReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }

        public async Task<IServiceReport<IEnumerable<GitLabel>>> CreateLabel(RepoModel repoModel, GitLabel newLable)
        {
            var url = $"{repoModel.url}/{LABELS}";

            var body = new
            {
                newLable.Name,
                newLable.description,
                newLable.color,
            };
            var getReport = await POST(url, body);
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>();

            if (!getReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }

        public async Task<IServiceReport<IEnumerable<GitLabel>>> UpdateLabel(RepoModel repoModel, GitLabel newLable)
        {
            var url = $"{repoModel.url}/{LABELS}/{newLable.Name}";
            var body = new
            {
                newLable.Name,
                newLable.description,
                newLable.color,
            };
            var getReport = await PATCH(url, body);
            var labelReport = new ServiceReport<IEnumerable<GitLabel>>();

            if (!getReport.IsFailed)
            {
                labelReport.Model = ParseResponse<GitLabel>(getReport.Model);
            }

            return labelReport;
        }
    }
}