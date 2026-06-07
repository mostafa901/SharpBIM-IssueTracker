using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

using SharpBIM.IssueTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.IssueTracker.Core.GitHttp
{
    public class GitRepos : GitClient
    {
        public GitRepos(IConfig appGlobals) : base(appGlobals)
        {
        }

        protected override string EndPoint => $"https://api.github.com/";

        protected override string GetEndPoint(params object[] values)
        {
            var vs = values.ToList();

            var url = base.GetEndPoint(vs.ToArray());
            return url;
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypesStatics.VNDGITHUBJSON));
        }

        public async Task<IServiceReport<string>> StarRepo(RepoModel repoModel)
        {
            string url = $"https://api.github.com/user/starred/{Owner}/{repoModel.name}";
            var response = await PUT(url, null);
            return response;
        }

        public async Task<IServiceReport<string>> IsRepoStared(RepoModel repoModel)
        {
            string url = $"https://api.github.com/user/starred/{Owner}/{repoModel.name}";
            var response = await GET(url);

            return response;
        }

        public async Task<IServiceReport<IEnumerable<RepoModel>>> GetRepos(string repoFullName = null)
        {
            var repoReport = new ServiceReport<IEnumerable<RepoModel>>();
            List<RepoModel> repos = new List<RepoModel>();
            int page = 1;
            int trials = 5;

            while (true)
            {
                string response = null;
                IServiceReport<string> getReport = new ServiceReport<string>();
                while (trials > 0)
                {
                    string url = $"{EndPoint}user/repos?page={page}";
                    if (!string.IsNullOrEmpty(repoFullName))
                    {
                        url = $"{EndPoint}repos/{repoFullName}";
                    }

                    getReport = await GET(url);
                    if (!getReport.IsFailed)
                    {
                        response = getReport.Model;
                        break;
                    }
                    trials--;
                }
                if (response == null)
                {
                    // refresh token is required.
                    repoReport.Merge(getReport);
                    return repoReport;
                }
                var importedRepos = ParseResponse<RepoModel>(response);
                if (importedRepos.Any() == false)
                    break;
                repos.AddRange(importedRepos);
                if (!string.IsNullOrEmpty(repoFullName))
                {
                    break;
                }
                    page++;
            }

            repoReport.Model = repos;
            return repoReport;
        }
    }
}