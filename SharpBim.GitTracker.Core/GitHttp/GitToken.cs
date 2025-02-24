using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.Auth.BrowseOptions;
using System.Text;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;
using static IdentityModel.OidcConstants;

namespace SharpBIM.GitTracker.GitHttp
{
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

        public async Task<string> AuthorizeApp()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitLoginOptions();

            var res = await brw.InvokeAsync(gitOps);
            return gitOps.Code;
        }

        public async Task<bool> RefreshToken()
        {
            var url = $"{endPoint}?client_id={Config.ClientId}&client_secret={Config.ClientSecret}&grant_type={QueryString.REFRESHTOKEN}&refresh_token={User.Token.refresh_token}";
            return await RequestToken(url, null);
        }

        public async Task<bool> RequestUserToken(string accessCode)
        {
            var url = $"{endPoint}?client_id={Config.ClientId}&client_secret={Config.ClientSecret}&code={accessCode}";
            return await RequestToken(url, null);
        }

        #endregion Public Methods

        #region Protected Methods

        protected override AuthenticationHeaderValue GetAuthentication()
        {
            return null;
        }

        #endregion Protected Methods

        #region Private Methods

        private async Task<bool> RequestToken(string url, object requestBody)
        {
            var report = await POST(url, requestBody);
            if (report.IsFailed)
                return false;
            var responseJson = report.Model;

            User.Token = ParseResponse<UserToken>(responseJson).FirstOrDefault();
            User.Token.ExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.expires_in);
            User.Token.RefreshExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.refresh_token_expires_in);
            return true;
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