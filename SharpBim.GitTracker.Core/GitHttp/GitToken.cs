using System.Net.Http.Headers;
using System.Net.Http;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.Auth.BrowseOptions;

namespace SharpBIM.GitTracker.GitHttp
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

        #endregion Protected Properties

        #region Private Properties

        private UserToken token => AppGlobals.user.Token;

        #endregion Private Properties

        #region Public Methods

        protected override bool NeedAuthentication => false;

        public async Task<IServiceReport<string>> AuthorizeApp()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitLoginOptions();

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

        #region Private Methods

        private async Task<IServiceReport<string>> RequestToken(string url, object requestBody)
        {
            var report = await POST(url, requestBody);
            if (report.IsFailed)
                return report;
            var responseJson = report.Model;

            User.Token = ParseResponse<UserToken>(responseJson).FirstOrDefault();
            User.Token.ExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.expires_in);
            User.Token.RefreshExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.refresh_token_expires_in);
            return report;
        }

        #endregion Private Methods

        protected override void AddHeaders(HttpRequestMessage request)
        {
            base.AddHeaders(request);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.VNDGITHUBJSON));
        }

        public async Task<IServiceReport<string>> CheckToken(string access_token)
        {
            string url = $"https://api.github.com/rate_limit";
            ////    string authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Config.ClientId}:{Config.ClientSecret}"));
            ////    RequestAuth = new AuthenticationHeaderValue("Basic", authToken);
            //var body = new
            //{
            //    access_token
            //};

            var report = await GET(url, null);

            return report;
        }

        protected override Task<bool> AreWeAuthorized()
        {
            return Task.FromResult(true);
        }
    }
}