using System.ComponentModel;
using SharpBIM.IssueTracker.Core.Enums;
using SharpBIM.ServiceContracts;
using SharpBIM.IssueTracker.Core.WPF.Mvvm.Views;
using SharpBIM.IssueTracker.Core.WPF.Helpers;
using System.Runtime.CompilerServices;
using SharpBIM.UIContext.Abstracts.Interfaces;
using SharpBIM.IssueTracker.Core.WPF.Views;

namespace SharpBIM.IssueTracker.Core.WPF.Mvvm.ViewModels
{
     
    public class IssueListViewModel : ModelViewBase<ModelBase, IssueViewModel>
    {
        #region Public Constructors
        protected override void FillContextCommands()
        {
            VisitRepoCommand = new SharpBIMCommand(VisitRepo, "Visit repo", Glyphs.link_vertical, (x) => true);
        }

        public IssueListViewModel()
        {
            LoadCommands();
            ColView.Filter = FilterIssues;
            ColView.SortDescriptions.Clear(); // Clear previous sorting
            ColView.SortDescriptions.Add(new SortDescription("Id", ListSortDirection.Descending));

            FetchIssueCounts = 200;
            PageNumber = 1;
            AutoFilterState = true;
            ProgressActivity = new();
        }

        public override async Task Init(ModelBase dataModel)
        {
            base.Init(dataModel);

            if (AppGlobals.User.RepoOwner == null || AppGlobals.User.RepoOwner.Length == 0)
            {
                RepoOwner = AppGlobals.User.UserAccount.login;
                AppGlobals.AppSettings.Save();
            }
            else
                RepoOwner = AppGlobals.User.RepoOwner;
            //      await ReloadRepos(null);
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
            if (issuemv.ContextData.labels.Any(o => o.Name.Contains(TextToFilter)))
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
                AppGlobals.AppSettings.Save();
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
                if (value != null && value == SelectedRepo)
                    return;
                SetValue(value, nameof(SelectedRepo));
                LoadIssuesAsync(null);
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
                    AppGlobals.AppSettings.Save();
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
                var report = new ServiceReport<string>("Failed to Edit Issue");
                report.Failed(ex);
                report.Show("Failed to Edit Issue");
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

                var pagen = Math.Max(1, PageNumber + 1);
                if (pagen != PageNumber)
                    LoadIssuesAsync(pagen);
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
                var pagen = Math.Max(1, PageNumber - 1);
                if (pagen != PageNumber)
                    LoadIssuesAsync(pagen);

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
                value = Math.Min(value, 100);
                if (value != FetchIssueCounts)
                {
                    SetValue(value, nameof(FetchIssueCounts));
                    LoadIssuesAsync(null);
                }
            }
        }

        public SharpBIMCommand CreateNewIssueCommand { get; set; }

        // Add this line to the constructor

        public async Task CreateNewIssue(object x)
        {
            string subissue = "Sub-Issue";
            string newissue = "New Issue";
            IServiceReport<string> ans = new ServiceReport<string>(newissue);

            try
            {
                if (SelectedRepo == null)
                    return;
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
                await EditIssue(issuemv);
            }
            catch (Exception ex)
            {
                ans.Failed(ex);
                ans.Show("Failed to Create Issue");
            }
        }

        public int TotalIssues
        {
            get { return GetValue<int>(nameof(TotalIssues)); }
            set { SetValue(value, nameof(TotalIssues)); }
        }



        public int TotalClosed
        {
            get { return GetValue<int>(nameof(TotalClosed)); }
            set { SetValue(value, nameof(TotalClosed)); }
        }



        public int TotalOpened
        {
            get { return GetValue<int>(nameof(TotalOpened)); }
            set { SetValue(value, nameof(TotalOpened)); }
        }

        public override SharpProgress ProgressActivity => GetParentViewModel<MainPageViewModel>()?.ProgressActivity;

        public virtual async Task LoadIssuesAsync(object x)
        {
            Children.Clear();

            if (ProgressActivity == null)
                return;
            ProgressActivity.Reset();
            List<IssueViewModel> issmvs = new();

            var pagenumber = PageNumber;
            if (x is int xx)
            {
                pagenumber = xx;
            }


            await Task.Run((Func<Task>)(async () =>
          {
              try
              {
                  Title = $"List of all {CurrentState} issues";

                  if ((SelectedRepo != null))
                  {
                      TotalOpened = SelectedRepo.open_issues_count;
                      if (SelectedRepo.has_issues)
                      {
                          await ProgressActivity.SetMessage("Fetching issues");


                          var issuesReport = await IssuesService.GetIssues(SelectedRepo, -1, CurrentState, pagenumber, FetchIssueCounts);

                          if (issuesReport.IsFailed)
                          {
                              AppGlobals.MsgService.AlertUser(WindowHandle, "Failed to Load", issuesReport.ErrorMessage);

                          }
                          else
                          {
#if true
                              var issues = issuesReport.Model;
                              PageNumber = pagenumber;
                              if (!issues.Any())
                              {
                                  PageNumber = Math.Max(1, PageNumber - 1);

                              }
                              foreach (var issue in issues)
                              {
                                  if (issue.labels != null && issue.labels.Any((Func<GitLabel, bool>)(o => o.Name == "sub-issue")))
                                      continue;
                                  if (issue.parent_issue_url != null)
                                  {
                                      continue;
                                  }
                                  var issueModel = Dispatcher.Invoke(() => issue.ToModelView<IssueViewModel>(this));

                                  issmvs.Add(issueModel);
                              }
#endif

                          }

                      }
                  }
                  foreach (var issueModel in issmvs.ToList())
                  {
                      if (issueModel.ContextData.sub_issues_summary.total != 0)
                      {
                          var subissues = (await IssuesService.GetSubIssues(SelectedRepo, issueModel.ContextData.number)).Model.Where(x => x.state == CurrentState.ToString());
                          var subIssueModels = Dispatcher.Invoke(() => subissues.ToModelViews<IssueViewModel>(issueModel).OrderBy(x => x.ContextData.number).Reverse());
                          var index = issmvs.IndexOf(issueModel);
                          for (int i = 0; i < subIssueModels.Count(); i++)
                          {
                              var subIssueModel = subIssueModels.ElementAt(i);
                              subIssueModel.PreFix = $"#{issueModel.ContextData.number} -> ";
                              issmvs.Insert(index + 1, subIssueModels.ElementAt(i));
                          }
                      }
                  }

                  await AddItemsAsync(issmvs, Token);

              }
              catch (Exception ex)
              {
              }
              finally
              {
                  ProgressActivity.Reset();
              }
          })
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


        public SharpBIMCommand VisitRepoCommand { get; set; }





        public void VisitRepo(object x)
        {
            try
            {
                if (SelectedRepo != null)
                    IOEx.OpenUrl(SelectedRepo.html_url);

            }
            catch (Exception ex)
            {
            }
        }
        public SharpBIMCommand ReloadReposCommand { get; set; }

        // Add this line to the constructor

        public async Task ReloadRepos(object x)
        {
            ProgressActivity.Reset();
            ProgressActivity.IsVisible = true;
            await ProgressActivity.SetMessage("Loading Repos");
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

        }
        public virtual async Task Reload(object x)
        {
            ProgressActivity.Reset();
            ProgressActivity.IsVisible = true;
            await ProgressActivity.SetMessage("Logging In");
            await Login();
            if (LoggedIn)
            {
                Children.Clear();
                await LoadOpenIssuesAsync(null);
            }
            else
            {
                ProgressActivity.Reset();
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

        async internal Task ReloadCount()
        {
            var repo = await ReposSerivce.GetRepos(SelectedRepo.full_name);
            TotalOpened = repo.Model.First().open_issues_count;
        }

        #endregion Public Methods
    }
}