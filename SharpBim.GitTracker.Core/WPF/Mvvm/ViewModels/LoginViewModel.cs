using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.GitTracker.Core.WPF.Views;
using SharpBIM.GitTracker.Core.GitHttp.Models;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels
{
    public class LoginViewModel : ModelViewBase<IUser>
    {
        public LoginViewModel()
        {
            AuthorizeCommand = new SharpBIMCommand(async (x) => await Authorize(x), "Authorize", Glyphs.empty, (x) => true);
            CancelCommand = new SharpBIMCommand(Cancel, "Cancel", Glyphs.empty, (x) => true);
        }

        public event EventHandler LoggedIn;

        public SharpBIMCommand AuthorizeCommand { get; set; }
        public string StoredToken { get; internal set; }

        public SharpBIMCommand CancelCommand { get; set; }

        // Add this line to the constructor

        public void Cancel(object x)
        {
            try
            {
                var vm = ParentModelView as MainPageViewModel;
                vm.CurrentView = null;
            }
            catch (Exception ex)
            {
            }
        }

        public bool AlreadyLoggedIn
        {
            get { return GetValue<bool>(nameof(AlreadyLoggedIn)); }
            set { SetValue(value, nameof(AlreadyLoggedIn)); }
        }

        public async Task Authorize(object x)
        {
            try
            {
                bool auth = false;
                AppGlobals.AppViewContext.UpdateProgress(1, 1, "Authorizing...", true);
                if (string.IsNullOrEmpty(StoredToken) || !AppGlobals.User.IsPersonalToken)
                {
                    // this is to rest the authentication and reautherize if needed

                    var login = await AuthService.Login();
                    if (login.IsFailed)
                    {
                        AppGlobals.MsgService.AlertUser(WindowHandle, "Login failed", login.ErrorMessage);
                    }
                    else
                    {
                        AppGlobals.User.IsPersonalToken = false;
                        auth = true;
                    }
                }
                else
                {
                    var userAccountReport = await AuthService.LoginByPersonalToken(StoredToken);
                    if (userAccountReport.IsFailed)
                    {
                        AppGlobals.MsgService.AlertUser(WindowHandle, "Invalid Token", userAccountReport.ErrorMessage);
                    }
                    else
                    {
                        auth = true;
                        AppGlobals.User.Token.access_token = StoredToken;
                        AppGlobals.User.IsPersonalToken = true;
                    }
                }
                if (auth)
                {
                    LoggedIn?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                AppGlobals.AppViewContext.UpdateProgress(1, 1, null, true);
            }
        }

        private void SaveUser()
        {
            AppGlobals.User.Save();
        }
    }
}