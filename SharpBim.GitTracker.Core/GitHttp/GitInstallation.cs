using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.Auth.BrowseOptions;
using System.IO;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitInstallation : GitClient
    {
        protected override string endPoint => "https://api.github.com/app/installations";

        protected override bool NeedAuthentication => false;

        public GitInstallation()
        {
        }

        private const string ResponseMissingPermission = "You do not have permission to perform this action. You need to reinstall the application.";
        private const string MissingGitPermission = "Resource not accessible by integration";

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            string jwtToken = Config.PrivateKey;
            request.Headers.Authorization = new AuthenticationHeaderValue(QueryString.BEARER, jwtToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.MACHINEMANPREVIEWJSON));
        }

        public async Task<IServiceReport<InstallationModel>> GetInstallationAsync()
        {
            var instReport = new ServiceReport<InstallationModel>();
            int trial = 5;
            InstallationModel installModel = null;
            while (trial > 0)
            {
                trial--;

                var report = await GET(endPoint);

                if (!report.IsFailed)
                {
                    var installModels = ParseResponse<InstallationModel>(report.Model);
                    installModel = installModels?.FirstOrDefault();
                    instReport.Model = installModel;
                    break;
                }
                else
                {
                    instReport.Merge(report);
                }
            }

            return instReport;
        }

        private string LimitURL = "https://api.github.com/rate_limit";

        public async Task GetLimits()
        {
            var response = await GET(LimitURL);
        }

        protected override async Task<bool> AreWeAuthorized()
        {
            return !(await AuthService.LoadGitConfig()).IsFailed;
        }

        public async Task<IServiceReport<AppModel>> GetApp()
        {
            string url = "https://api.github.com/app";
            IServiceReport<AppModel> repModel = new ServiceReport<AppModel>();
            var rep = await GET(url);
            if (rep.IsFailed)
            {
                // something with my private key or JWT token is wrong.. app will not work
                return repModel.Merge(rep);
            }
            var appModel = ParseResponse<AppModel>(rep.Model).FirstOrDefault(o => o.client_id == Config.ClientId);
            repModel.Model = appModel;
            return repModel;
        }

        public async Task<bool> RequestInstalling()
        {
            var brw = new SystemBrowser();
            var gitOps = new GitInstallOptions();

            var res = await brw.InvokeAsync(gitOps);
            if (res.ResultType == IdentityModel.OidcClient.Browser.BrowserResultType.Success)
            {
                return !string.IsNullOrEmpty(gitOps.InstallationId);
            }
            return false;
        }
    }
}