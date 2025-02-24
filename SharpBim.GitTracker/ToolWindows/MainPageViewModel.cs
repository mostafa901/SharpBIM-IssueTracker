using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft;
using SharpBim.GitTracker.Helpers;
using SharpBim.GitTracker.Mvvm.ViewModels;
using SharpBim.GitTracker.Mvvm.Views;
using SharpBIM.GitTracker;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.GitTracker.Mvvm.ViewModels;
using SharpBIM.GitTracker.Mvvm.Views;
using SharpBIM.UIContexts;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBim.GitTracker.ToolWindows
{
    public class MainPageViewModel : ModelViewBase
    {
        public SharpBIMCommand NavigateBackCommand { get; set; }

        // Add this line to the constructor

        public MainPageViewModel()
        {
            NavigateBackCommand = new SharpBIMCommand(NavigateBack, "NavigateBack", Glyphs.empty, (x) => true);
            NavigateForwardCommand = new SharpBIMCommand(NavigateForward, "Navigate Forward", Glyphs.empty, (x) => true);
            ShowLoginScreenCommand = new SharpBIMCommand(async (x) => await ShowLoginScreen(x), "Login", Glyphs.login, (x) => true);
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
                var grantted = !((await AuthService.Login()).IsFailed);
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
        }

        public async Task ShowLoginScreen(object x)
        {
            try
            {
                var vm = new LoginViewModel() { ParentModelView = this };
                vm.AlreadyLoggedIn = x == null ? true : ((await AuthService.Login()).IsFailed);

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