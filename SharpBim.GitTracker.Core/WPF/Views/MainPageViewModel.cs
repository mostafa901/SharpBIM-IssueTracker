using System.Windows;
using System.Windows.Controls;
using SharpBIM.UIContexts;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Views;

namespace SharpBIM.GitTracker.Core.WPF.Views
{
    public class MainPageViewModel : ModelViewBase
    {
        public SharpBIMCommand NavigateBackCommand { get; set; }

        // Add this line to the constructor

        public MainPageViewModel()
        {
            NavigateBackCommand = new SharpBIMCommand(NavigateBack, "NavigateBack", Glyphs.empty, (x) => true);
            NavigateForwardCommand = new SharpBIMCommand(NavigateForward, "Navigate Forward", Glyphs.empty, (x) => true);
            ShowLoginScreenCommand = new SharpBIMCommand(async (x) => await ShowLoginScreen(null), "Login", Glyphs.login, (x) => true);
            IsLoginScreen = true;
        }

        public SharpBIMCommand ShowLoginScreenCommand { get; set; }

        public bool IsLoginScreen
        {
            get { return GetValue<bool>(nameof(IsLoginScreen)); }
            set { SetValue(value, nameof(IsLoginScreen)); }
        }

        public string ProgressMessage
        {
            get { return GetValue<string>(nameof(ProgressMessage)); }
            set { SetValue(value, nameof(ProgressMessage)); }
        }

        public async Task Login(object x)
        {
            try
            {
                AppGlobals.AppViewContext.UpdateProgress(1, 1, "Logging In", true);

                var gitconfReport = await AuthService.LoadGitConfig();
                if (gitconfReport.IsFailed)
                {
                    AppGlobals.MsgService.AlertUser(WindowHandle, "Couldn't login", gitconfReport.ErrorMessage);
                }

                bool grantted = !gitconfReport.IsFailed;
                if (grantted)
                {
                    var accessReport = await AuthService.Login(AppGlobals.User);
                    grantted = !(accessReport.IsFailed);
                }

                if (!grantted)
                {
                    ShowLoginScreenCommand.Hint = "Login";
                    ShowLoginScreenCommand.Icon = Glyphs.login;
                    ShowLoginScreenCommand.Appearance = ButtonType.Transparent;
                    await ShowLoginScreen(grantted);
                }
                else
                {
                    ViewModel_LoggedIn(null, null);
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

        public async Task ShowLoginScreen(object x)
        {
            try
            {
                var vm = new LoginViewModel() { ParentModelView = this };
                vm.AlreadyLoggedIn = x == null ? true : (!((bool)x) || ((await AuthService.Login(AppGlobals.User)).IsFailed));

                vm.LoggedIn += ViewModel_LoggedIn;
                CurrentView = new LoginView();
                CurrentView.DataContext = vm;
            }
            catch (Exception ex)
            {
            }
        }

        public FrameworkElement CurrentView
        {
            get { return GetValue<FrameworkElement>(nameof(CurrentView)); }
            set { SetValue(value, nameof(CurrentView)); }
        }

        public bool ShowProgressBar
        {
            get { return GetValue<bool>(nameof(ShowProgressBar)); }
            set { SetValue(value, nameof(ShowProgressBar)); }
        }

        public async void ViewModel_LoggedIn(object sender, EventArgs e)
        {
            AppGlobals.User.Save();
            IsLoginScreen = false;
            CurrentView = null;
            ShowLoginScreenCommand.Icon = Glyphs.logout;
            ShowLoginScreenCommand.Hint = "Logout";
            ShowLoginScreenCommand.Appearance = ButtonType.Success;
            var vm = new IssueListViewModel() { ParentModelView = this, LoggedIn = true };
            AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueListView), vm);
            await vm.ReloadRepos(null);
        }

        public SharpBIMCommand NavigateForwardCommand { get; set; }

        // Add this line to the constructor

        public void NavigateForward(object x)
        {
            try
            {
                var frame = x as Frame;

                frame.GoForward();
            }
            catch (Exception ex)
            {
            }
        }

        public void NavigateBack(object x)
        {
            try
            {
                var frame = x as Frame;
                frame.GoBack();
            }
            catch (Exception ex)
            {
            }
        }
    }
}