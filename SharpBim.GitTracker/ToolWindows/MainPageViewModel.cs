using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpBIM.GitTracker;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.GitTracker.Mvvm.ViewModels;
using SharpBIM.UIContexts;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBim.GitTracker.ToolWindows
{
    public class MainPageViewModel : ModelViewBase
    {
        #region Public Constructors

        public MainPageViewModel()
        {
            LoginCommand = new SharpBIMCommand(async (x) => await Reload(x), "Refresh", Glyphs.reload, (x) => true);
            EditIssueCommand = new SharpBIMCommand(EditIssue, "Edit", Glyphs.empty, (x) => true);
        }

        public SharpBIMCommand EditIssueCommand { get; set; }

        public void EditIssue(object x)
        {
            try
            {
            }
            catch (Exception ex)
            {
            }
        }

        #endregion Public Constructors

        #region Public Properties

        public IssueState CurrentState { get; set; } = IssueState.open;

        public ObservableCollection<IssueViewModel> IssueModels { get; set; } = [];

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
                    LoginCommand.Appearance = SharpBIM.WPF.Helpers.ButtonType.Success;
                }
                else
                    LoginCommand.Appearance = SharpBIM.WPF.Helpers.ButtonType.Secondary;
            }
        }

        public SharpBIMCommand LoginCommand { get; set; }
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

        public void LoadClosedIssues(object x)
        {
            try
            {
                if (CurrentState == IssueState.closed && IssueModels.Any())
                    return;
                CurrentState = IssueState.closed;
                LoadIssues(null);
            }
            catch (Exception ex)
            {
            }
        }

        public async void LoadIssues(object x)
        {
            try
            {
                IssueModels.Clear();
                if ((SelectedRepo != null))
                {
                    if (SelectedRepo.has_issues)
                    {
                        var issues = await IssuesService.GetIssues(SelectedRepo.name, -1, CurrentState);
                        if (issues != null)
                            foreach (var issue in issues)
                            {
                                var issmv = issue.ToModelView<IssueViewModel>(this);
                                IssueModels.Add(issmv);

                                issmv.Notify(nameof(IssueViewModel.IsClosed));
                            }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async void LoadOpenIssues(object x)
        {
            try
            {
                if (CurrentState == IssueState.open && IssueModels.Any())
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
                if (await SharpGitTracker.Login())
                {
                    LoggedIn = true;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task Reload(object x)
        {
            await Login();
            if (LoggedIn)
            {
                RepoModels.Clear();
                IssueModels.Clear();
                var repos = await ReposSerivce.GetRepos();
                string storedName = Properties.Settings.Default.LastRepoName;
                SelectedRepo = string.IsNullOrEmpty(storedName) ? repos.FirstOrDefault() : repos.FirstOrDefault(o => o.name == storedName);
                foreach (var repo in repos)
                {
                    RepoModels.Add(repo);
                }
            }
        }

        #endregion Public Methods
    }
}