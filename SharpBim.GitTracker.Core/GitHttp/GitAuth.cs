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
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.Core.Properties;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitAuth : GitClient
    {
        public GitAuth()
        {
        }

        public void SaveUser()
        {
            Settings.Default.USERJSON = User.JSerialize();
            Settings.Default.Save();
        }

        public void LoadUser()
        {
            if (User == null)
            {
                string jsonUser = Settings.Default.USERJSON;
                AppGlobals.user = new User();

                if (!string.IsNullOrEmpty(jsonUser))
                {
                    AppGlobals.user = User.Parse(jsonUser);
                     
                }
            }
        }

        private bool RequiresToken => User.Token == null || User.Token.access_token == null || User.Token.ExpireTime.Ticks < DateTime.Now.Ticks ;

        public async Task<IServiceReport<bool>> Login()
        {
            LoadUser();
            var report = new ServiceReport<bool>();
            try
            {
                bool loginResult = true;
                if (RequiresToken)
                {
                    if (User.Token != null && User.Token.RefreshExpireTime.Ticks > DateTime.Now.Ticks)
                    {
                        loginResult = await TokenService.RefreshToken();
                    }
                    if (!loginResult)
                    {
                        var accessCode = await TokenService.AuthorizeApp();
                        if (string.IsNullOrEmpty(accessCode))
                        {
                            // user did not authorize the app
                            report.Failed("User did not authorize the app");
                        }
                        else
                        {
                            loginResult = await TokenService.RequestUserToken(accessCode);

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

                if (string.IsNullOrEmpty(User.InstallationId))
                {
                    var installationmodel = await InstallService.GetInstallationIdAsync();
                    if (installationmodel == null)
                    {
                        // user has not installed the app.
                        // do we need to?
                        loginResult = await InstallService.RequestInstalling();
                        if (loginResult)
                            installationmodel = await InstallService.GetInstallationIdAsync();
                    }

                    User.Installation = installationmodel;
                }
            if (!report.IsFailed)
                SaveUser();
            }
            catch (Exception ex)
            {
                report.Failed(ex);
            }
            return report;
        }
    }
}