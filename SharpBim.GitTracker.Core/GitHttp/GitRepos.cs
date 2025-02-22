using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using SharpBIM.GitTracker.Auth;

namespace SharpBIM.GitTracker.GitHttp
{
    public class GitRepos : GitClient
    {
        //  protected override string endPoint => $"{AppGlobals.user.Installation.account.repos_url}?type=private";
        protected override string endPoint => $"https://api.github.com/user/repos";

        internal GitRepos()
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        public async Task<IEnumerable<RepoModel>> GetRepos()
        {
            List<RepoModel> repos = new List<RepoModel>();
            int page = 1;
            int trials = 5;
            while (true)
            {
                string response = null;
                while (trials > 0)
                {
                    var url = $"{endPoint}?page={page}";

                    var report = await GET(url);
                    if (!report.IsFailed)
                    {
                        response = report.Model;
                        break;
                    }
                    trials--;
                }
                if (response == null)
                {
                    // refresh token is required.
                    await AuthService.Login();
                    return repos;
                }
                var importedRepos = JsonSerializer.Deserialize<IEnumerable<RepoModel>>(response);
                if (importedRepos.Any() == false)
                    break;
                repos.AddRange(importedRepos);
                page++;
            }

            return repos;
        }
    }
}