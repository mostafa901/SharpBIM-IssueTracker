using System.Configuration;

#if WINDOWS
using IdentityModel.OidcClient; 
#endif

using Microsoft;

using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.GitHttp;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Utility.Helpers;

namespace SharpBIM.GitTracker.Core.GitHttp
{
    public class GitAuth : GitClient
    {
        #region Public Constructors

        public GitAuth(IConfig appGlobals) : base(appGlobals)
        {
        }

        #endregion Public Constructors

        #region Private Properties

        private bool RequiresToken => User.Token == null || User.Token.access_token == null || User.Token.ExpireTime.Ticks < DateTime.Now.Ticks;

        #endregion Private Properties

        #region Public Methods

        protected override bool NeedAuthentication => false;

        public async Task<IServiceReport<Account>> GetUserAccount()
        {
            var accountReport = new ServiceReport<Account>();
            var url = "https://api.github.com/user";
            var res = await GET(url);
            if (res.IsFailed)
            {
                accountReport.Merge(res);
                return accountReport;
            }
            var account = ParseResponse<Account>(res.Model).FirstOrDefault();

            accountReport.Model = account;
            return accountReport;
        }

        public async Task<IServiceReport<string>> LoadGitConfigAsync()
        {
            var configReport = new ServiceReport<string>();
            if (Config == null)
            {
                AppGlobals.SharpUser = User;
                NeedAuthentication = false;
                UseLocal = false;


                NeedAuthentication = false;
                var confReport = await GetGitConfigAsync();

                if (confReport.IsFailed)
                {
                    Config = null;
                    configReport.Merge(confReport);
                }
                else
                {
                    Config = confReport.Model;
                }
            }
            return configReport;
        }

#if WINDOWS
        public async Task<IServiceReport<string>> Login()
        {
            IServiceReport<string> loginReport = new ServiceReport<string>();


            try
            {
                Config ??= (await GetGitConfigAsync()).Model;
                GitTrackerGlobals.AppGlobals.User = GitUser.Parse();

                if (User.Token != null && User.Token.access_token != null)
                {
                    if (User.Token.ExpireTime < DateTime.Now && User.Token.refresh_token != null && User.Token.RefreshExpireTime.Ticks > DateTime.Now.Ticks)
                    {
                        loginReport = await TokenService.RefreshToken();
                    }
                }

                else
                {
                    loginReport.Failed("No token");
                }
                if (loginReport.IsFailed)
                {
                    loginReport = await AuthService.GetGitInstallationTokenAsync();
                    if (loginReport.IsFailed)
                    {

                        return loginReport;
                    }
                    User.Token.access_token = loginReport.Model;

                    var appReport = await InstallService.GetApp();
                    if (appReport.IsFailed)
                    {
                        var installReport = await InstallService.RequestInstallingAsync();
                        if (installReport.IsFailed)
                        {
                            // user canceled installation
                            return installReport;
                        }
                    }
                    User.Installation = (await InstallService.GetInstallationAsync()).Model;
                    loginReport = await TokenService.AuthorizeApp();
                    var accesCode = loginReport.Model;
                    loginReport = await TokenService.RequestAppUserToken(accesCode);
                    User.UserAccount = appReport.Model.owner;
                    UpdateOwnerAccount(User.UserAccount.login);

                }

            }
            catch (Exception ex)
            {
                loginReport.Failed(ex);
            }
            return loginReport;
        }

#endif
        public async Task<IServiceReport<string>> LoginByPersonalToken(string userAccesToken, string secret = "")
        {
            var report = new ServiceReport<string>();

            //User.IsPersonalToken = true;
            User.Token.access_token = userAccesToken;
            if (secret != string.Empty)
            {
                User.UserSecret = secret;
            }
            if (string.IsNullOrEmpty(User.Name))
            {
                var accountReport = await GetUserAccount();
                if (!accountReport.IsFailed)
                {
                    report.Model = accountReport.Model.JSerialize();
                    User.Name = accountReport.Model.login;
                }
                else
                    report.Merge(accountReport);
            }
            return report;
        }

        #endregion Public Methods

        #region Private Methods

        public async Task<IServiceReport<string>> GetJWTTokenAsync(string useremail)
        {
            var url = $"{EndPoint}/auth/gettoken";
            var resposne = await POST(url, useremail);

            return resposne;
        }


        #endregion Private Methods
    }
}