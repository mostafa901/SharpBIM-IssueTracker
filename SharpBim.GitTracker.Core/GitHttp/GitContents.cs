using System.Net.Http;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.UIContexts;

namespace SharpBIM.GitTracker.GitHttp
{
    public class GitContents : GitClient
    {
        protected override string endPoint => $"https://api.github.com/repos/{Account.login}/REPO/contents";

        //either delete,or create or update, no concurrent actions

        public async Task<IServiceReport<string>> DeleteFile(string repoName, ContentModel contentModel)
        {
            IServiceReport<string> report = new ServiceReport<string>();

            var url = $"{GetEndPoint(repoName)}";
            var body = new
            {
                message = "Delete file",
                contentModel.sha,
                // branch name, we are using the default at the moment
            };
            report = await DELETE($"{url}/{contentModel.path}", body);

            return report;
        }

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        public async Task<IServiceReport<ContentModel>> GetFile(string repoName, string filePath)
        {
            var url = $"{GetEndPoint(repoName)}/{filePath}";

            var report = await base.GET(url);
            if (!report.IsFailed)
            {
                var model = ParseResponse<ContentModel>(report.Model).FirstOrDefault();
                return new ServiceReport<ContentModel>() { Model = model };
            }

            return null;
        }
    }
}