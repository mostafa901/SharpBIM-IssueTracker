using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using SharpBim.GitTracker.Mvvm.Views;
using SharpBim.GitTracker.ToolWindows;
using SharpBIM.GitTracker.Auth;
using SharpBIM.UIContexts;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBim.GitTracker.Mvvm.ViewModels
{
    public class LoginViewModel : ModelViewBase<User>
    {
        public LoginViewModel()
        {
            AuthService.LoadUser();
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
                if (StoredToken == null)
                {
                    var login = await AuthService.Login();
                    if (login.IsFailed)
                    {
                        AppGlobals.MsgService.AlertUser(WindowHandle, "Login failed", login.ErrorMessage);
                    }
                    else
                    {
                        auth = true;
                    }
                }
                else
                {
                    // StoredToken = AppGlobals.User.Token.access_token;
                    if (!string.IsNullOrEmpty(StoredToken))
                    {
                        var checkReport = await TokenService.CheckToken(StoredToken);
                        if (checkReport.IsFailed)
                        {
                            AppGlobals.MsgService.AlertUser(WindowHandle, "Invalid Token", checkReport.ErrorMessage);
                        }
                        else
                            auth = true;
                    }
                    else
                    {
                        var rep = await AuthService.Login();
                        if (rep.IsFailed)
                        {
                            AppGlobals.MsgService.AlertUser(WindowHandle, "Invalid Token", rep.ErrorMessage);
                        }
                        else
                            auth = true;
                    }
                }
                if (auth)
                {
                    if (StoredToken.Length > 0)
                    {
                        AppGlobals.User.IsPersonalToken = true;

                        AppGlobals.User.Token.access_token = StoredToken;
                    }
                    else
                    {
                        AppGlobals.User.IsPersonalToken = false;
                    }
                    AuthService.SaveUser();
                    LoggedIn?.Invoke(this, null);
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}