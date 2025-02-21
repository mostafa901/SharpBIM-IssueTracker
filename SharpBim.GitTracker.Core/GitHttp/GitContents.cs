using System.Net.Http;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.UIContexts;

namespace SharpBIM.GitTracker.GitHttp
{
    public class GitContents : GitClient
    {
        protected override string endPoint => $"https://api.github.com/repos/{Account.login}/REPO/contents";

        public async Task<IServiceReport<ContentModel>> GetFile(RepoModel repo, string filePath)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{endPoint.Replace("REPO", repo.name)}/{filePath}");

            var report = await base.GET(request);
            if (!report.IsFailed)
            {
                var model = ParseResponse<ContentModel>(report.Model).FirstOrDefault();
                return new ServiceReport<ContentModel>() { Model = model };
            }

            return null;
        }
    }
}