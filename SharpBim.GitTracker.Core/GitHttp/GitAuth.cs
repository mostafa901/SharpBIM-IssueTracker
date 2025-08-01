using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.GitTracker.Core.GitHttp;
using System.Configuration;
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
                var confReport = await GetGitConfigAsync(User.UserSecret);
                if (confReport.IsFailed)
                {
                    Config = null;
                    configReport.Merge(confReport);
                }
                else
                {
                    Config = confReport.Model;

#if WINDOWS
                    InstallService = new(AppGlobals); 
#endif
                }
            }
            return configReport;
        }

        public async Task<IServiceReport<string>> Login()
        {
            var report = new ServiceReport<string>();

            if ((await LoadGitConfigAsync()).IsFailed)
            {
                return report.Failed("Application missing credentials");
            }
            try
            {
                if (User.Name == null)
                {
                    var isInstalledRep = await IsGitTrackerInstalled();
                    if (isInstalledRep.IsFailed)
                    {
                        return report.Merge(isInstalledRep);
                    }

                    if (RequiresToken)

                    {
                        var TokenService = new GitToken(AppGlobals);
                        var loginResult = false;

                        if (User.Token != null && User.Token.ExpireTime < DateTime.Now && User.Token.refresh_token != null && User.Token.RefreshExpireTime.Ticks > DateTime.Now.Ticks)
                        {
                            var refreshReport = await TokenService.RefreshToken();
                            loginResult = !refreshReport.IsFailed;
                        }
                        if (!loginResult)
                        {
                            var accessCodeReport = await TokenService.AuthorizeApp();
                            if (accessCodeReport.IsFailed)
                            {
                                // user did not authorize the app
                                report.Merge(accessCodeReport);
                                return report;
                            }

                            var accesCode = accessCodeReport.Model;
                            var userTokenReport = await TokenService.RequestUserToken(accesCode);

                            loginResult = !userTokenReport.IsFailed;
                            if (loginResult == false)
                            {
                                // user did not authorize the app
                                report.Merge(userTokenReport);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                report.Failed(ex);
            }
            return report;
        }

        public async Task<IServiceReport<string>> LoginByPersonalToken(string userAccesToken, string secret = "")
        {
            var report = new ServiceReport<string>();
      
            //User.IsPersonalToken = true;
            User.Token.access_token = userAccesToken;
            if (secret != string.Empty)
            {
                User.UserSecret  = secret;
            }
            if (User.Name == null)
            {
                var accountReport = await GetUserAccount();
                report.Merge(accountReport);
                User.Name = accountReport.Model.login;
            }
            return report;
        }

        #endregion Public Methods

        #region Private Methods

        private async Task<IServiceReport<string>> IsGitTrackerInstalled()
        {
            var isInstallReport = new ServiceReport<string>();
#if WINDOWS
            if (User.Installation == null)
            {
                // check if the app already authorized
                var getAppRep = await InstallService.GetApp();
                if (getAppRep.IsFailed)
                {
                    return isInstallReport.Merge(getAppRep);
                }
                else if (getAppRep.Model.installations_count == 0)
                {
                    // user has not installed the app.
                    if (!await InstallService.RequestInstallingAsync())
                    {
                        isInstallReport.Failed("Failed to install the application");
                        return isInstallReport;
                    }
                }

                var getInsModelReport = await InstallService.GetInstallationAsync();
                if (getInsModelReport.IsFailed)
                {
                    return isInstallReport.Merge(getInsModelReport);
                }

                User.Installation = getInsModelReport.Model;
            }
            User.UserAccount = User.Installation.account; 
#endif
            return isInstallReport;
        }

        #endregion Private Methods
    }
}