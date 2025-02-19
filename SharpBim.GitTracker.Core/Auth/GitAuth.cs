using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Net.Http;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using SharpBim.GitTracker.Auth.BrowseOptions;
using SharpBim.GitTracker.GitHttp;
using SharpBIM.Utility.Extensions;

namespace SharpBim.GitTracker.Auth
{
    public class GitAuth
    {
        private IGitConfig Config => AppGlobals.Config;
        private GitToken TokenClient;
        private GitInstallation InstallClient;

        public User user => AppGlobals.user;
        public UserToken? token => user?.Token;

        public GitAuth(string jsonUser)
        {
            LoadUser(jsonUser);
        }

        private void LoadUser(string jsonUser)
        {
            AppGlobals.user = new User();

            if (!string.IsNullOrEmpty(jsonUser))
            {
                AppGlobals.user = User.Parse(jsonUser);
                if (token != null)
                {
                    if (token.expires_in >= DateTime.Now.Ticks)
                    {
                        AppGlobals.user = new User();
                    }
                }
            }
        }

        private bool RequiresToken => token == null || token.ExpireTime.Ticks < DateTime.Now.Ticks || token.RefreshExpireTime.Ticks < DateTime.Now.Ticks;

        public async Task<bool> Login()
        {
            bool loginResult = true;
            if (RequiresToken)
            {
                TokenClient ??= new GitToken();
                if (token == null)
                {
                    var accessCode = await TokenClient.AuthorizeApp();
                    loginResult = await TokenClient.RequestUserToken(accessCode);
                }
                else if (token.RefreshExpireTime.Ticks < DateTime.Now.Ticks)
                {
                    loginResult = await TokenClient.RefreshToken();
                }
            }
            if (string.IsNullOrEmpty(user.InstallationId))
            {
                InstallClient ??= new GitInstallation();
                var installationmodel = await InstallClient.GetInstallationIdAsync();
                if (installationmodel == null)
                {
                    // user has not installed the app.
                    // do we need to?
                    loginResult = await InstallClient.RequestInstalling();
                    if (loginResult)
                        installationmodel = await InstallClient.GetInstallationIdAsync();
                }

                AppGlobals.user.Installation = installationmodel;
            }

            return loginResult;
        }
    }
}