using System.Collections.ObjectModel;
using System.ComponentModel;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.UIContexts;
using SharpBIM.UIContexts.Abstracts.Interfaces;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Views;
using SharpBIM.GitTracker.Core.WPF.Helpers;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using Microsoft;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels
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
            Title = "List of all issues";
            LoadCommands();
            ColView.Filter = FilterIssues;
            ColView.SortDescriptions.Clear(); // Clear previous sorting
            ColView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

            FetchIssueCounts = 200;
            PageNumber = 1;
            AutoFilterState = true;
        }

        public override async void Init(DummyListContext dataModel)
        {
            base.Init(dataModel);

            if (AppGlobals.User.RepoOwner == null || AppGlobals.User.RepoOwner.Length == 0)
            {
                RepoOwner = AppGlobals.User.UserAccount.login;
                AppGlobals.User.Save();
            }
            else
                RepoOwner = AppGlobals.User.RepoOwner;
            await ReloadRepos(null);
        }

        public string TextToFilter
        {
            get { return GetValue<string>(nameof(TextToFilter)); }
            set
            {
                SetValue(value, nameof(TextToFilter));
                ColView.Filter = FilterIssues;
            }
        }

        private bool FilterIssues(object obj)
        {
            if (string.IsNullOrWhiteSpace(TextToFilter))
                return true;

            var issuemv = obj as IssueViewModel;
            if (issuemv == null)
                return false;

            if ((issuemv.ContextData.number + issuemv.ContextData.body?.ToString() + issuemv.ContextData.Title).Contains(TextToFilter))
                return true;
            if (issuemv.ContextData.labels.Any(o => o.name.Contains(TextToFilter)))
                return true;

            return false;
        }

        public string RepoOwner
        {
            get { return GetValue<string>(nameof(RepoOwner)); }
            set
            {
                if (string.IsNullOrEmpty(value))
                    value = AppGlobals.User.UserAccount.login;
                SetValue(value, nameof(RepoOwner));
                IssuesService.UpdateOwnerAccount(value);
                AppGlobals.User.RepoOwner = value;
                AppGlobals.User.Save();
                ReloadRepos(null);
            }
        }

        private void LoadCommands()
        {
            CreateNewIssueCommand = new SharpBIMCommand(async (x) => await CreateNewIssue(x), "Create New Issue", Glyphs.plus_circle, (x) => true);
            ReloadCommand = new SharpBIMCommand(async (x) => await Reload(x), "Refresh", Glyphs.reload, (x) => true);
            EditIssueCommand = new SharpBIMCommand(async (x) => await EditIssue(x), "Edit", Glyphs.edit, (x) => true);
            LoadCurrentProjectCommand = new SharpBIMCommand(LoadCurrentProject, "Sync to Current Project", Glyphs.arrows_swap, (x) => true);
            LessPageCommand = new SharpBIMCommand(LessPage, "Less Page", Glyphs.arrow_60_left, (x) => true);
            MorePageCommand = new SharpBIMCommand(MorePage, "More Page", Glyphs.arrow_60_right, (x) => true);
            ReloadReposCommand = new SharpBIMCommand(async (x) => await ReloadRepos(x), "Reload Repos", Glyphs.reload_sm, (x) => true);
            LoadClosedIssuesCommand = new SharpBIMCommand(async (x) => await LoadClosedIssuesAsync(x), "Get Closed Issues", null, (x) => true);
            LoadOpenIssuesCommand = new SharpBIMCommand(async (x) => await LoadOpenIssuesAsync(x), "Get Open Issues", Glyphs.folder_open, (x) => true);
        }

        #endregion Public Constructors

        public bool AutoFilterState
        {
            get { return GetValue<bool>(nameof(AutoFilterState)); }
            set { SetValue(value, nameof(AutoFilterState)); }
        }

        #region Public Properties

        public IssueState CurrentState { get; set; } = IssueState.open;

        public SharpBIMCommand EditIssueCommand { get; set; }

        public SharpBIMCommand LoadClosedIssuesCommand { get; set; }

        public SharpBIMCommand LoadOpenIssuesCommand { get; set; }

        public bool LoggedIn
        {
            get { return GetValue<bool>(nameof(LoggedIn)); }
            set
            {
                SetValue(value, nameof(LoggedIn));
            }
        }

        public SharpBIMCommand ReloadCommand { get; set; }
        public ObservableCollection<RepoModel> RepoModels { get; set; } = [];

        public virtual RepoModel SelectedRepo
        {
            get { return GetValue<RepoModel>(nameof(SelectedRepo)); }
            set
            {
                SetValue(value, nameof(SelectedRepo));
            }
        }

        protected override async void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals(nameof(SelectedRepo)))
            {
                if (!string.IsNullOrEmpty(SelectedRepo?.name))
                {
                    AppGlobals.User.LastRepoName = SelectedRepo.name;
                    AppGlobals.User.Save();
                    await LoadIssuesAsync(null);
                }
            }
        }

        public IssueViewModel SelectedIssueModel
        {
            get { return GetValue<IssueViewModel>(nameof(SelectedIssueModel)); }
            set { SetValue(value, nameof(SelectedIssueModel)); }
        }

        #endregion Public Properties

        #region Public Methods

        public async Task EditIssue(object x)
        {
            try
            {
                var issueVM = x as IssueViewModel;
                if (issueVM == null)
                    return;
                await issueVM.LoadDetails();
                AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueView), issueVM);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task LoadClosedIssuesAsync(object x)
        {
            try
            {
                if (CurrentState == IssueState.closed && Children.Any())
                    return;
                CurrentState = IssueState.closed;
                await LoadIssuesAsync(null);
            }
            catch (Exception ex)
            {
            }
        }

        public int PageNumber
        {
            get { return GetValue<int>(nameof(PageNumber)); }
            set { SetValue(value, nameof(PageNumber)); }
        }

        public SharpBIMCommand MorePageCommand { get; set; }

        // Add this line to the constructor

        public void MorePage(object x)
        {
            try
            {
                PageNumber = Math.Min(FetchIssueCounts / 100, PageNumber++);
            }
            catch (Exception ex)
            {
            }
        }

        public SharpBIMCommand LessPageCommand { get; set; }

        // Add this line to the constructor

        public bool CanChangeRepo => typeof(IssueListViewModel) == (this.GetType());

        public void LessPage(object x)
        {
            try
            {
                PageNumber = Math.Max(1, --PageNumber);
            }
            catch (Exception ex)
            {
            }
        }

        // multiples of 100
        public int FetchIssueCounts
        {
            get { return GetValue<int>(nameof(FetchIssueCounts)); }
            set
            {
                value = (int)((Math.Ceiling((double)value / 100) * 100));
                SetValue(value, nameof(FetchIssueCounts));
            }
        }

        public SharpBIMCommand CreateNewIssueCommand { get; set; }

        // Add this line to the constructor

        public async Task CreateNewIssue(object x)
        {
            try
            {
                string subissue = "Sub-Issue";
                string newissue = "New Issue";
                IServiceReport<string> ans = new ServiceReport<string>(newissue);
                if (SelectedIssueModel != null)
                {
                    ans = AppGlobals.MsgService.AlertUser(WindowHandle, "Create New issue", $"Do you want to create a new Issue", [newissue, subissue, Statics.CANCEL], SharpBIM.ServiceContracts.Enums.MessageType.Info);

                    if (ans.Model == Statics.CANCEL)
                    {
                        return;
                    }
                }
                var issuemodel = new IssueModel();
                issuemodel.Title = "";
                issuemodel.Id = -1;

                var issuemv = issuemodel.ToModelView<IssueViewModel>(this);
                if (ans.Model == subissue)
                {
                    issuemv.ParentModelView = SelectedIssueModel;
                    await SelectedIssueModel.AddItemsAsync([issuemv], Token);
                }
                EditIssue(issuemv);
            }
            catch (Exception ex)
            {
            }
        }

        public virtual async Task LoadIssuesAsync(object x)
        {
            Children.Clear();
            AppGlobals.AppViewContext.UpdateProgress(0, 0, "Fetching issues", true);
            List<IssueViewModel> issmvs = new();
            await Task.Run(async () =>
          {
              try
              {
                  if ((SelectedRepo != null))
                  {
                      if (SelectedRepo.has_issues)
                      {
                          int pCount = FetchIssueCounts / 100;
                          for (int i = 0; i < pCount; i++)
                          {
                              var issuesReport = await IssuesService.GetIssues(SelectedRepo.name, -1, CurrentState, i + ((PageNumber - 1) * pCount));

                              if (issuesReport.IsFailed)
                              {
                                  AppGlobals.MsgService.AlertUser(WindowHandle, "Failed to Load", issuesReport.ErrorMessage);
                              }
                              else
                              {
                                  var issues = issuesReport.Model;
                                  List<long> addedIds = [];
                                  foreach (var issue in issues)
                                  {
                                      if (issue.labels.Any(o => o.name == "sub-issue"))
                                          continue;
                                      if (addedIds.Any(o => o == issue.number))
                                          continue;
                                      var issueModel = Dispatcher.Invoke(() => issue.ToModelView<IssueViewModel>(this));

                                      issmvs.Add(issueModel);
                                  }

                                  await AddItemsAsync(issmvs, Token);
                                  if (!issues.Any() || issues.Count() < FetchIssueCounts)
                                  {
                                      break;
                                  }
                              }
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
          }
        );
        }

        public async Task LoadOpenIssuesAsync(object x)
        {
            try
            {
                if (CurrentState == IssueState.open && Children.Any())
                    return;
                CurrentState = IssueState.open;

                await LoadIssuesAsync(null);
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

        public SharpBIMCommand ReloadReposCommand { get; set; }

        // Add this line to the constructor

        public async Task ReloadRepos(object x)
        {
            AppGlobals.AppViewContext.UpdateProgress(1, 1, "Loading Repos", true);
            RepoModels.Clear();
            Children.Clear();
            IEnumerable<RepoModel> repoModels = [];
            await Task.Run(async () =>
                 {
                     try
                     {
                         await Task.Delay(50);
                         var getRepoReport = await ReposSerivce.GetRepos();
                         if (getRepoReport.IsFailed)
                         {
                             AppGlobals.MsgService.AlertUser(WindowHandle, "Couldn't Get Repositories", getRepoReport.ErrorMessage);
                             return;
                         }

                         repoModels = getRepoReport.Model;
                     }
                     catch (Exception ex)
                     {
                     }
                     finally
                     {
                     }
                 });

            foreach (var repo in repoModels)
            {
                RepoModels.Add(repo);
            }

            string storedName = AppGlobals.User.LastRepoName;
            SelectedRepo = RepoModels.FirstOrDefault(o => o.name.EQ(storedName))
                ?? RepoModels.FirstOrDefault(o => o.has_issues)
                ?? RepoModels.FirstOrDefault();

            if (SelectedRepo == null)
                AppGlobals.AppViewContext.UpdateProgress(1, 1, null, true);
        }

        public virtual async Task Reload(object x)
        {
            AppGlobals.AppViewContext.UpdateProgress(1, 1, "Logging In", true);
            await Login();
            if (LoggedIn)
            {
                Children.Clear();
                await LoadOpenIssuesAsync(null);
            }
            else
            {
                AppGlobals.AppViewContext.UpdateProgress(0, 0, null, true);
            }
        }

        #endregion Public Methods
    }
}