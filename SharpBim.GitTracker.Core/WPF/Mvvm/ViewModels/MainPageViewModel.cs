using System.Windows;
using System.Windows.Controls;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Views;
using SharpBIM.Utility.Extensions;
using System.Threading.Tasks;
using SharpBIM.WPF.Controls.Dialogs;
using System.Windows.Media;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using System.Text.Json;
using SharpBIM.ServiceContracts.Interfaces;

namespace SharpBIM.GitTracker.Core.WPF.Views
{
    public class MainPageViewModel : ModelViewBase
    {
        public SharpBIMCommand NavigateBackCommand { get; set; }

        // Add this line to the constructor

        public MainPageViewModel()
        {
            VisitMarketPlaceCommand = new SharpBIMCommand(VisitMarketPlace, "Navigate to market place", Glyphs.empty, (x) => true);
            NavigateBackCommand = new SharpBIMCommand(NavigateBack, "NavigateBack", Glyphs.empty, (x) => true);
            NavigateForwardCommand = new SharpBIMCommand(NavigateForward, "Navigate Forward", Glyphs.empty, (x) => true);
            CheckForUpdatesCommand = new SharpBIMCommand(async (x) => await CheckForUpdates(x), "Check for updates", Glyphs.empty, (x) => true);
            ShowLoginScreenCommand = new SharpBIMCommand(async (x) => await ShowLoginScreen(null), "Login", Glyphs.login, (x) => true);
            FeedBackCommand = new SharpBIMCommand(async (x) => await FeedBack(x), "Feedback", Glyphs.question, (x) => true);
            StarRepoCommand = new SharpBIMCommand(async (x) => await StarRepo(x), "Star me", Glyphs.star_outline, (x) => true);
            IsLoginScreen = true;
            var ver = this.GetType().Assembly.GetName().Version;
            Version = $"{ver.Major}.{ver.Minor}";
            ProgressActivity = new();
            ProgressActivity.FillBrush = ResourceValues.SolidColorBrushs.ControlBlueThemeBrush;
        }

        public bool IsCheckingForUpdate
        {
            get { return GetValue<bool>(nameof(IsCheckingForUpdate)); }
            set { SetValue(value, nameof(IsCheckingForUpdate)); }
        }

        public SharpBIMCommand CheckForUpdatesCommand { get; set; }

        // Add this line to the constructor

        public async Task CheckForUpdates(object x)
        {
            try
            {
                if (IsCheckingForUpdate)
                    return;
                IsCheckingForUpdate = true;
                await Task.Delay(200);
                var appReport = await InstallService.GetApp();
                if (!appReport.IsFailed)
                {
                    var app = appReport.Model;
                    InstallService.UpdateOwnerAccount(app.owner.login);
                    string repoName = app.slug;
                    var repoModel = await GetTrackerRepo();
                    var releaseReport = await ReleaseService.GetLatestRelease(repoModel);
                    if (releaseReport.Model != null)
                    {
                        var releaseModel = releaseReport.Model;
                        if (releaseModel.tag_name != Version)
                        {
                            NewVersionLink = new Uri("https://marketplace.visualstudio.com/items?itemName=SharpBIM.SharpBIMGitTracker");
                            Version = releaseModel.tag_name;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                InstallService.UpdateOwnerAccount(AppGlobals.User.RepoOwner);
                IsCheckingForUpdate = false;
            }
        }

        public SharpBIMCommand VisitMarketPlaceCommand { get; set; }

        public void VisitMarketPlace(object x)
        {
            try
            {
                IOEx.OpenUrl(NewVersionLink.ToString());
            }
            catch (Exception ex)
            {
            }
        }

        public Uri NewVersionLink
        {
            get { return GetValue<Uri>(nameof(NewVersionLink)); }
            set { SetValue(value, nameof(NewVersionLink)); }
        }

        public string Version
        {
            get { return GetValue<string>(nameof(Version)); }
            set { SetValue(value, nameof(Version)); }
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

        public SharpBIMCommand FeedBackCommand { get; set; }


        public async Task FeedBack(object x)
        {
            try
            {
                var ans = AppGlobals.MsgService.FeedBack(WindowHandle, "Feedback", "");
                if (!ans.IsFailed && ans.Model != null)
                {
                    var issuemodel = new IssueModel
                    {
                        Title = ans.Model.Item1,
                        body = ans.Model.Item2
                    };
                    var repoModel = await GetTrackerRepo();
                    if (repoModel != null)
                    {
                        var respons = await IssuesService.CreateIssue(repoModel, issuemodel);
                        if (respons.IsFailed)
                        {
                            AppGlobals.MsgService.AlertUser(WindowHandle, "Failed to send feedback", respons.ErrorMessage);
                        }
                        else
                        {
                            AppGlobals.MsgService.AlertUser(WindowHandle, "Thanks", "Thanks for your feedback");
                        }
                    }
                }

                //IOEx.OpenUrl("https://github.com/mostafa901/SharpBIM.GitTracker/issues");
            }
            catch (Exception ex)
            {
            }
        }

        RepoModel trackerRepo = null;
        private async Task<RepoModel> GetTrackerRepo()
        {
            return trackerRepo ?? (trackerRepo = (await new GitRepos(AppGlobals).GetRepos()).Model.FirstOrDefault(x => x.name.EQ(AppGlobals.ApplicationDisplayName)));
        }

        public SharpBIMCommand StarRepoCommand { get; set; }

        // Add this line to the constructor

        public async Task StarRepo(object x)
        {
            try
            {
                var repoModel = await GetTrackerRepo();
                var response = await ReposSerivce.IsRepoStared(repoModel);

                if (response.IsFailed)
                {
                    StarRepoCommand.Icon = Glyphs.star_outline;
                    response = await ReposSerivce.StarRepo(repoModel);
                    if (!response.IsFailed)
                        StarRepoCommand.Icon = Glyphs.star;
                }
            }
            catch (Exception ex)
            {
            }
        }
        bool firstlogin = true;
        public async Task Login(object x)
        {
            bool grantted = false;
            try
            {
                ProgressActivity.Reset();
                ProgressActivity.IsVisible = true;
                ProgressActivity.Title = "Logging In";

                if (!firstlogin)
                {
                    grantted = AppGlobals.User.LoggedIn;
                    if (grantted)
                    {
                        var accessReport = await AuthService.Login();
                        grantted = !(accessReport.IsFailed);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                ProgressActivity.Reset();
            }

            if (!grantted)
            {
                AppGlobals.User.LoggedIn = false;
                AppGlobals.User.Save();
                ShowLoginScreenCommand.Hint = "Login";
                ShowLoginScreenCommand.Icon = Glyphs.login;
                await ShowLoginScreen(grantted);

            }
            else
            {
                ViewModel_LoggedIn(null, null);
            }
        }

        public async Task ShowLoginScreen(object x)
        {
            try
            {
                var vm = new LoginViewModel() { ParentModelView = this };
                vm.AlreadyLoggedIn = AppGlobals.User.LoggedIn;

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
            AppGlobals.User.LoggedIn = true;
            AppGlobals.User.Save();
            AuthService.UpdateOwnerAccount(AppGlobals.User.RepoOwner);
            firstlogin = false;
            IsLoginScreen = false;
            CurrentView = null;
            ShowLoginScreenCommand.Icon = Glyphs.logout;
            ShowLoginScreenCommand.Hint = "Logout";
            var vm = new IssueListViewModel() { ParentModelView = this, LoggedIn = true };
            AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueListView), vm);

            await vm.Init(new DummyModelBase());
            await CheckForUpdates(null);
            var repo = new ServiceReport<string>();
            var repoModel = await GetTrackerRepo();
            var response = await ReposSerivce.IsRepoStared(repoModel);
            if (!response.IsFailed)
            {
                //StarRepoCommand.Icon = Glyphs.empty;
                StarRepoCommand.IsVisible = false;
            }
        }

        public SharpBIMCommand NavigateForwardCommand { get; set; }

        // Add this line to the constructor

        public void NavigateForward(object x)
        {
            try
            {
                var frame = x as Frame;

                frame?.GoForward();
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
                var issueModel = ((frame?.Content as Page)?.Content as IssueView)?.DataContext as IssueViewModel;
                if(issueModel!=null)
                {
                    if(issueModel.ParentModelView is IssueViewModel parent)
                    {
                        parent.ReloadIssue(null);
                    }
                }
                frame?.GoBack();

            }
            catch (Exception ex)
            {
            }
        }

        public override IServiceReport<bool> Save()
        {
            throw new NotImplementedException();
        }

        public override IServiceReport<bool> Validate()
        {
            throw new NotImplementedException();
        }
    }
}