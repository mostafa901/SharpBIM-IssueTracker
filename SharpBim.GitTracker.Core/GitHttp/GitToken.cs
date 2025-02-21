using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.Auth.BrowseOptions;

namespace SharpBIM.GitTracker.GitHttp
{
    internal class GitToken : GitClient
    {
        private UserToken token => AppGlobals.user.Token;

        public GitToken()
        {
        }

        public async Task<string> AuthorizeApp()
        {
            // check if the app already authorized
            var brw = new SystemBrowser();
            var gitOps = new GitLoginOptions();

            var res = await brw.InvokeAsync(gitOps);
            return gitOps.Code;
        }

        protected override string endPoint => $"https://github.com/login/oauth/access_token";

        public async Task<bool> RequestUserToken(string accessCode)
        {
            var requestBody = new
            {
                client_id = Config.ClientId,
                client_secret = Config.ClientSecret,
                code = accessCode
            };

            return await RequestToken(endPoint, requestBody);
        }

        public async Task<bool> RefreshToken()
        {
            var requestBody = new
            {
                client_id = Config.ClientId,
                client_secret = Config.ClientSecret,
                grant_type = QueryString.REFRESHTOKEN,
                refresh_token = token.refresh_token
            };

            return await RequestToken(endPoint, requestBody);
        }

        private async Task<bool> RequestToken(string url, object requestBody)
        {
            var responseJson = await POST(url, requestBody);
            if (responseJson == null)
                return false;
            using var doc = JsonDocument.Parse(responseJson);
            User.Token = JsonSerializer.Deserialize<UserToken>(responseJson);
            User.Token.ExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.expires_in);
            User.Token.RefreshExpireTime = DateTime.Now.AddSeconds(AppGlobals.user.Token.refresh_token_expires_in);
            return true;
        }
    }
}