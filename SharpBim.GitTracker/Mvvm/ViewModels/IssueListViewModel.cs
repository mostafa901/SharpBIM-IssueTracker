using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SharpBim.GitTracker.Helpers;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.GitTracker.Mvvm.ViewModels;
using SharpBIM.GitTracker.Mvvm.Views;
using SharpBIM.UIContexts;
using SharpBIM.UIContexts.Abstracts.Interfaces;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBim.GitTracker.Mvvm.ViewModels
{
    public class DummyListContext : IModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }

    public class IssueListViewModel : ModelViewBase<DummyListContext, IssueViewModel>
    {
        #region Public Constructors

        public IssueListViewModel()
        {
            ReloadCommand = new SharpBIMCommand(async (x) => await Reload(x), "Refresh", Glyphs.reload, (x) => true);
            EditIssueCommand = new SharpBIMCommand(EditIssue, "Edit", Glyphs.edit, (x) => true);
            LoadCurrentProjectCommand = new SharpBIMCommand(LoadCurrentProject, "Load Current Project", Glyphs.arrows_swap, (x) => true);
            AutoFilterCommand = new SharpBIMCommand(AutoFilter, "Disallow AutoFilter", Glyphs.filter, (x) => true, ButtonType.Success);
            AutoFilterState = true;
        }

        #endregion Public Constructors

        public SharpBIMCommand AutoFilterCommand { get; set; }

        public void AutoFilter(object x)
        {
            try
            {
                AutoFilterCommand.Hint = "Disallow AutoFilter";
                AutoFilterCommand.Appearance = ButtonType.Success;
                if (AutoFilterState)
                {
                    AutoFilterCommand.Hint = "Allow AutoFilter";
                    AutoFilterCommand.Appearance = ButtonType.Secondary;
                }

                AutoFilterState = !AutoFilterState;
            }
            catch (Exception ex)
            {
            }
        }

        public bool AutoFilterState
        {
            get { return GetValue<bool>(nameof(AutoFilterState)); }
            set { SetValue(value, nameof(AutoFilterState)); }
        }

        #region Public Properties

        public IssueState CurrentState { get; set; } = IssueState.open;

        public SharpBIMCommand EditIssueCommand { get; set; }

        public SharpBIMCommand LoadClosedIssuesCommand => new SharpBIMCommand(LoadClosedIssues, "Get Closed Issues", null, (x) => true);

        public SharpBIMCommand LoadOpenIssuesCommand => new SharpBIMCommand(LoadOpenIssues, "Get Open Issues", Glyphs.folder_open, (x) => true);

        public bool LoggedIn
        {
            get { return GetValue<bool>(nameof(LoggedIn)); }
            set
            {
                SetValue(value, nameof(LoggedIn));
                if (LoggedIn)
                {
                    ReloadCommand.Appearance = SharpBIM.WPF.Helpers.ButtonType.Success;
                }
                else
                    ReloadCommand.Appearance = SharpBIM.WPF.Helpers.ButtonType.Secondary;
            }
        }

        public SharpBIMCommand ReloadCommand { get; set; }
        public ObservableCollection<RepoModel> RepoModels { get; set; } = [];

        public RepoModel SelectedRepo
        {
            get { return GetValue<RepoModel>(nameof(SelectedRepo)); }
            set
            {
                SetValue(value, nameof(SelectedRepo));
                if (!string.IsNullOrEmpty(value?.name))
                {
                    Properties.Settings.Default.LastRepoName = value.name;
                    Properties.Settings.Default.Save();
                }
                LoadOpenIssues(null);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public void EditIssue(object x)
        {
            try
            {
                var issueVM = x as IssueViewModel;
                issueVM.LoadDetails();
                AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueView), issueVM);
            }
            catch (Exception ex)
            {
            }
        }

        public void LoadClosedIssues(object x)
        {
            try
            {
                if (CurrentState == IssueState.closed && Children.Any())
                    return;
                CurrentState = IssueState.closed;
                LoadIssues(null);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task LoadIssues(object x)
        {
            Children.Clear();
            AppGlobals.AppViewContext.UpdateProgress(0, 0, "Fetching issues", true);
            Task.Run(async () =>
            {
                try
                {
                    if ((SelectedRepo != null))
                    {
                        if (SelectedRepo.has_issues)
                        {
                            var issues = await IssuesService.GetIssues(SelectedRepo.name, -1, CurrentState);
                            if (issues != null)
                            {
                                await AddItemsAsync(issues.Select(o => o.ToModelView<IssueViewModel>(this)), Token);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    AppGlobals.AppViewContext.UpdateProgress(0, 0, null, true);
                }
            });
        }

        public async void LoadOpenIssues(object x)
        {
            try
            {
                if (CurrentState == IssueState.open && Children.Any())
                    return;
                CurrentState = IssueState.open;

                LoadIssues(null);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task Login()
        {
            try
            {
                LoggedIn = false;
                var loginReport = await AuthService.Login();
                if (loginReport.IsFailed)
                {
                    throw new Exception(loginReport.ErrorMessage);
                }
                else
                {
                    LoggedIn = true;
                }
            }
            catch (Exception ex)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Login Failed", ex.Message);
            }
        }

        public SharpBIMCommand LoadCurrentProjectCommand { get; set; }

        public void LoadCurrentProject(object x)
        {
            try
            {
                var currentRepoName = GitHelper.GetRepoName(GitHelper.GetGitRepositoryPath());
                if (!string.IsNullOrEmpty(currentRepoName))
                {
                    var availableRepo = RepoModels.FirstOrDefault(o => o.name == currentRepoName);
                    if (availableRepo == null)
                    {
                        AppGlobals.MsgService.AlertUser(WindowHandle, "Repo not available", "The current project has no access to Git Repo");
                    }
                    else
                        SelectedRepo = availableRepo;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task Reload(object x)
        {
            AppGlobals.AppViewContext.UpdateProgress(0, 0, "Logging In", true);
            await Login();
            if (LoggedIn)
            {
                RepoModels.Clear();
                Children.Clear();
                var repos = await ReposSerivce.GetRepos();
                string storedName = Properties.Settings.Default.LastRepoName;
                SelectedRepo = string.IsNullOrEmpty(storedName) ? repos.FirstOrDefault() : repos.FirstOrDefault(o => o.name == storedName);
                foreach (var repo in repos)
                {
                    RepoModels.Add(repo);
                }
            }
            else
            {
                AppGlobals.AppViewContext.UpdateProgress(0, 0, null, true);

            }
        }

        #endregion Public Methods
    }
}