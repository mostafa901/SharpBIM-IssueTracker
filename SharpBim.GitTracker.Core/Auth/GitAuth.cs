using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using System.Net.Http;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using SharpBIM.GitTracker.Auth.BrowseOptions;
using SharpBIM.GitTracker.GitHttp;
using SharpBIM.Utility.Extensions;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;

namespace SharpBIM.GitTracker.Auth
{
    public class GitAuth
    {
        private IGitConfig Config => AppGlobals.Config;
        private GitToken TokenClient;
        private GitInstallation InstallClient;

        public User user => AppGlobals.user;
        public UserToken? token => user?.Token;

        public GitAuth()
        {
            TokenClient = new GitToken();
            InstallClient = new GitInstallation();
        }

        internal void LoadUser(string jsonUser)
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

        public async Task<IServiceReport<bool>> Login()
        {
            var report = new ServiceReport<bool>();
            try
            {
                bool loginResult = true;
                if (RequiresToken)
                {
                    if (token != null && token.RefreshExpireTime.Ticks > DateTime.Now.Ticks)
                    {
                        loginResult = await TokenClient.RefreshToken();
                    }
                    else
                    {
                        var accessCode = await TokenClient.AuthorizeApp();
                        if (string.IsNullOrEmpty(accessCode))
                        {
                            // user did not authorize the app
                            report.Failed("User did not authorize the app");
                        }
                        else
                        {
                            loginResult = await TokenClient.RequestUserToken(accessCode);

                            if (loginResult == false)
                            {
                                // user did not authorize the app
                                report.Failed("User did not authorize the app");
                            }
                            else
                            {
                                // user has authorized the app
                                // we have a new token
                            }

                            // a barand new tokken is required
                        }
                    }
                }

                if (string.IsNullOrEmpty(user.InstallationId))
                {
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
            }
            catch (Exception ex)
            {
                report.Failed(ex);
            }

            return report;
        }
    }
}