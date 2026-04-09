using System.Net.Http;

using SharpBIM.ServiceContracts;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitContents : GitClient
    {
        public GitContents(IConfig appGlobals) : base(appGlobals)
        {
        }

       // protected override string EndPoint => $"https://api.github.com/repos/{Owner}/REPO/contents";

        protected override string GetEndPoint(params object[] values)
        {
            var vs = values.ToList();
            vs.Insert(1, "contents");
            var url = base.GetEndPoint(vs.ToArray());
            return url;
        }
        //either delete,or create or update, no concurrent actions

        public async Task<IServiceReport<string>> DeleteFile(RepoModel repoModel, ContentModel contentModel)
        {
            IServiceReport<string> report = new ServiceReport<string>();

            string url = GetEndPoint(repoModel);
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
            var url = GetEndPoint(repoName,filePath);

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