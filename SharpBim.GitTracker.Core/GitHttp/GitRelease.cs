using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitRelease : GitClient
    {
        protected override string endPoint => $"https://api.github.com/repos/{User.UserAccount.login}/REPO/releases";

#if false
        public async Task<IServiceReport<ReleaseNoteModel>> GenerateRelease(string repoName, string tag_name)
        {
            var url = $"{GetEndPoint(repoName)}/generate-notes";
            var body = new
            {
                tag_name
            };

            var releaseReport = new ServiceReport<ReleaseNoteModel>();
            var response = await POST(url, body);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseNoteModel>(response.Model);
                releaseReport.Model = models.FirstOrDefault();
            }
            return releaseReport;
        }

        public async Task<IServiceReport<IEnumerable<ReleaseNoteModel>>> GetReleases(string repoName)
        {
            var url = GetEndPoint(repoName);
            var releaseReport = new ServiceReport<IEnumerable<ReleaseNoteModel>>();
            var response = await GET(url);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseNoteModel>(response.Model);
                releaseReport.Model = models;
            }

            return releaseReport;
        }
#endif

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> GetReleasesByTag(string repoName, string tag)
        {
            var url = $"{GetEndPoint(repoName)}/tags/{tag}";
            var releaseReport = new ServiceReport<IEnumerable<ReleaseModel>>();
            var response = await GET(url);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseModel>(response.Model);
                releaseReport.Model = models;
            }

            return releaseReport;
        }

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> CreateRelease(string repoName, ReleaseModel noteModel)
        {
            var url = GetEndPoint(repoName);
            var releaseReport = new ServiceReport<IEnumerable<ReleaseModel>>();
            var body = new
            {
                noteModel.tag_name,
                noteModel.name,
                noteModel.body,
                noteModel.draft
            };

            var response = await POST(url, body);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseModel>(response.Model);
                releaseReport.Model = models;
            }

            return releaseReport;
        }

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> UpdateRelease(string repoName, ReleaseModel noteModel)
        {
            var url = $"{GetEndPoint(repoName)}/{noteModel.id}";
            var releaseReport = new ServiceReport<IEnumerable<ReleaseModel>>();
            var body = new
            {
                noteModel.tag_name,
                noteModel.body,
                noteModel.name,
                noteModel.draft
            };

            var response = await POST(url, body);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseModel>(response.Model);
                releaseReport.Model = models;
            }

            return releaseReport;
        }

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> GetAssets(string repoName, ReleaseModel releaseModel)
        {
            var url = $"{GetEndPoint(repoName)}/{releaseModel.id}/assets";
            var releaseReport = new ServiceReport<IEnumerable<ReleaseModel>>();
            var body = new
            {
                releaseModel.tag_name,
                releaseModel.body,
                releaseModel.draft
            };

            var response = await GET(url);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseModel>(response.Model);
                releaseReport.Model = models;
            }

            return releaseReport;
        }
    }
}