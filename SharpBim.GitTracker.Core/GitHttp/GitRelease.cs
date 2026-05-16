using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitRelease : GitClient
    {
        public GitRelease(IConfig appGlobals) : base(appGlobals)
        {
        }

        //  protected override string EndPoint => $"https://api.github.com/repos/{User.Name}/REPO/releases";

        protected override string GetEndPoint(params object[] values)
        {
            var vs = values.ToList();
            vs.Insert(1, "releases");
            var url = base.GetEndPoint(vs.ToArray());
            return url;
        }

#if false
        public async Task<IServiceReport<ReleaseNoteModel>> GenerateRelease(RepoModel repoModel, string tag_name)
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

        public async Task<IServiceReport<IEnumerable<ReleaseNoteModel>>> GetReleases(RepoModel repoModel)
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

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> GetReleasesByTag(RepoModel repoModel, string tag)
        {
            var url = string.Empty;
            if (!string.IsNullOrEmpty(tag))
                url = GetEndPoint(repoModel, "tags", tag);
            else
                url = GetEndPoint(repoModel);

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

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> CreateRelease(RepoModel repoModel, ReleaseModel noteModel)
        {
            var url = GetEndPoint(repoModel);
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

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> UpdateRelease(RepoModel repoModel, ReleaseModel noteModel)
        {
            var url = GetEndPoint(repoModel, noteModel.id);
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

        public async Task<IServiceReport<ReleaseModel>> GetLatestRelease(RepoModel repoModel)
        {
            var url = GetEndPoint(repoModel, "latest");
            var releaseReport = new ServiceReport<ReleaseModel>();

            var response = await GET(url);
            if (response.IsFailed)
            {
                releaseReport.Merge(response);
            }
            else
            {
                var models = ParseResponse<ReleaseModel>(response.Model);
                releaseReport.Model = models.LastOrDefault();
            }

            return releaseReport;
        }

        public async Task<IServiceReport<IEnumerable<ReleaseModel>>> GetAssets(RepoModel repoModel, ReleaseModel releaseModel)
        {
            var url = GetEndPoint(repoModel, releaseModel.id, "assets");
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