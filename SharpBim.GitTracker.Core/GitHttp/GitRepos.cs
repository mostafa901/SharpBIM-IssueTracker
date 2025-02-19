using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using SharpBim.GitTracker.Auth;

namespace SharpBim.GitTracker.GitHttp
{
    public class GitRepos : GitClient
    {
        //  protected override string endPoint => $"{AppGlobals.user.Installation.account.repos_url}?type=private";
        protected override string endPoint => $"https://api.github.com/user/repos";

        public GitRepos()
        {
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        public async Task<IEnumerable<RepoModel>?> GetRepos()
        {
            List<RepoModel> repos = new List<RepoModel>();
            int page = 1;
            int trials = 5;
            while (true)
            {
                string response = null;
                while (trials > 0)
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{endPoint}?page={page}");

                    response = await GET(request);
                    if (response != null)
                        break;
                }
                if (response == null)
                {
                    return null;
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