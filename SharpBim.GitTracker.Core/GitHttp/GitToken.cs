using System.Net.Http.Headers;
using System.Net.Http;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.Auth.BrowseOptions;
using SharpBIM.Services;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    /// <summary>
    /// Reference https://docs.github.com/en/apps/creating-github-apps/authenticating-with-a-github-app/generating-a-user-access-token-for-a-github-app
    /// </summary>
    public class GitToken : GitClient
    {
        #region Public Constructors

        public GitToken()
        {
        }

        #endregion Public Constructors

        #region Protected Properties

        protected override string endPoint => $"https://github.com/login/oauth/access_token";

        protected override bool NeedAuthentication => false;

        #endregion Protected Properties

        #region Public Methods

        public async Task<IServiceReport<string>> AuthorizeApp()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitLoginOptions();
            gitOps.DisplayMode = IdentityModel.OidcClient.Browser.DisplayMode.Hidden;

            var res = await brw.InvokeAsync(gitOps);
            var report = new ServiceReport<string>();
            if (res.ResultType == IdentityModel.OidcClient.Browser.BrowserResultType.Success)
            {
                report.Model = gitOps.Code;
            }
            else
            {
                report.Failed(res.Error);
            }
            return report;
        }

        public async Task<IServiceReport<string>> GetAppAccessCode()
        {
            string url = $"https://github.com/login/oauth/authorize?client_id={AppGlobals.Config.ClientId}&state=xxx";

            var accessCodeReport = await GET(url);
            return accessCodeReport;
        }

        public async Task<IServiceReport<string>> RefreshToken()
        {
            var url = $"{endPoint}?client_id={Config.ClientId}&client_secret={Config.ClientSecret}&grant_type={QueryString.REFRESHTOKEN}&refresh_token={User.Token.refresh_token}";
            var tokenReport = await RequestToken(url, null);
            return tokenReport;
        }

        public async Task<IServiceReport<string>> RequestUserToken(string accessCode)
        {
            var url = $"{endPoint}?client_id={Config.ClientId}&client_secret={Config.ClientSecret}&code={accessCode}";
            return await RequestToken(url, null);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        protected override async Task<bool> AreWeAuthorized()
        {
            if (User.IsPersonalToken)
                return true;
            return !(await AuthService.LoadGitConfigAsync()).IsFailed;
        }

        #endregion Protected Methods

        #region Private Methods

        private async Task<IServiceReport<string>> RequestToken(string url, object requestBody)
        {
            var report = await POST(url, requestBody);
            if (report.IsFailed)
                return report;
            var responseJson = report.Model;

            User.Token = ParseResponse<SharpToken>(responseJson).FirstOrDefault();
            User.Token.ExpireTime = DateTime.Now.AddSeconds(AppGlobals.User.Token.expires_in);
            User.Token.RefreshExpireTime = DateTime.Now.AddSeconds(AppGlobals.User.Token.refresh_token_expires_in);
            return report;
        }

        #endregion Private Methods
    }
}