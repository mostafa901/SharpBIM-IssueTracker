using System.Net.Http;
using System.Net.Http.Headers;
using SharpBIM.IssueTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.IssueTracker.Core.Auth;
using SharpBIM.IssueTracker.Core.Auth.BrowseOptions;
using System.Threading.Tasks;
 
namespace SharpBIM.IssueTracker.Core.GitHttp
{
    public class GitInstallation : GitClient
    {
        protected override string EndPoint => "https://api.github.com/app/installations";

        protected override bool NeedAuthentication => false;



        private const string ResponseMissingPermission = "You do not have permission to perform this action. You need to reinstall the application.";
        private const string MissingGitPermission = "Resource not accessible by integration";

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);

            request.Headers.Authorization ??= new AuthenticationHeaderValue(QueryString.BEARER, AppGlobals.SharpUser.Token.access_token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypesStatics.MACHINEMANPREVIEWJSON));
        }

        public async Task<IServiceReport<InstallationModel>> GetInstallationAsync()
        {
            var instReport = new ServiceReport<InstallationModel>();
            int trial = 5;
            InstallationModel installModel = null;
            while (trial > 0)
            {
                trial--;

                var report = await GET(EndPoint);

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

        public GitInstallation(IConfig appGlobals) : base(appGlobals)
        {
        }

        public async Task GetLimits()
        {
            var response = await GET(LimitURL);
        }

        
        protected override async Task<AuthenticationHeaderValue> GetAuthentication()
        {
            
            var auth = new AuthenticationHeaderValue(SharpBIM.Statics.BEARER, (await  new GitAuth(AppGlobals).GetGitInstallationTokenAsync()).Model);

            return auth;

        }
        protected override async Task<bool> AreWeAuthorized()
        {
            return true;
        }

        public async Task<IServiceReport<AppModel>> GetApp()
        {
            string url = "https://api.github.com/app";
            IServiceReport<AppModel> repModel = new ServiceReport<AppModel>();
            var rep = await GET(url);
            if (rep.IsFailed)
            {
                // something with my private key or JWT token is wrong.. app will not work
                  repModel.Failed(rep.ErrorMessage);
                  return repModel;
            }
            var appModel = ParseResponse<AppModel>(rep.Model).FirstOrDefault(o => o.name == AppGlobals.ApplicationName);
            repModel.Model = appModel;
            return repModel;
        }
         
        public async Task<IServiceReport<string>> RequestInstallingAsync()
        {
            var report = new ServiceReport<string>();
#if WINDOWS
            var brw = new SystemBrowser();
            var gitOps = new GitInstallOptions();

            var res = await brw.InvokeAsync(gitOps);
            if (res.ResultType == IdentityModel.OidcClient.Browser.BrowserResultType.Success)
            {
                if (string.IsNullOrEmpty(gitOps.InstallationId))
                {
                    report.Failed("Installation failed");
                }
                else report.Model = gitOps.InstallationId;
            }
#endif
            return report;
        }
    }
}