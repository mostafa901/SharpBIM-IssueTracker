using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitRepos : GitClient
    {
        //  protected override string endPoint => $"{AppGlobals.User.Installation.account.repos_url}?type=private";
        protected override string endPoint => $"https://api.github.com/user";

        internal GitRepos()
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        public async Task<IServiceReport<string>> StarRepo(string repoName)
        {
            string url = $"https://api.github.com/user/starred/{Owner}/{repoName}";
            var response = await PUT(url, null);
            return response;
        }

        public async Task<IServiceReport<string>> IsRepoStared(string repoName)
        {
            string url = $"https://api.github.com/user/starred/{Owner}/{repoName}";
            var response = await GET(url);

            return response;
        }

        public async Task<IServiceReport<IEnumerable<RepoModel>>> GetRepos()
        {
            var repoReport = new ServiceReport<IEnumerable<RepoModel>>();
            List<RepoModel> repos = new List<RepoModel>();
            int page = 1;
            int trials = 5;
            var baseurl = $"{endPoint}s/{Owner}/repos";
            //https://api.github.com/users/USERNAME/repos
            while (true)
            {
                string response = null;
                IServiceReport<string> getReport = new ServiceReport<string>();
                while (trials > 0)
                {
                    string url = $"{endPoint}/repos?page={page}";
                    if (Owner != AppGlobals.User.UserAccount.login)
                        url = $"{baseurl}?page={page}&sort=full_name";

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
                    return repoReport.Merge(getReport);
                }
                var importedRepos = JsonSerializer.Deserialize<IEnumerable<RepoModel>>(response);
                if (importedRepos.Any() == false)
                    break;
                repos.AddRange(importedRepos);
                page++;
            }
            if (repos.Any())
                if (AppGlobals.User.IsPersonalToken)
                {
                    if (AppGlobals.User.UserAccount == null)
                    {
                        AppGlobals.User.UserAccount = repos.First().owner;
                    }
                }
            repoReport.Model = repos;
            return repoReport;
        }
    }
}