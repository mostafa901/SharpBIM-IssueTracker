﻿using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SharpBIM.GitTracker.Core.Enums;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Enums;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts.QAQC;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Models;
using SharpBIM.GitTracker.Core.WPF.Mvvm.Views;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using System.ComponentModel;
using System.Windows.Data;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels
{
    public class IssueViewModel : ModelViewBase<IssueModel, IssueViewModel>
    {
        #region Private Fields

        private bool detailsLoaded;
        private Dictionary<string, ContentModel> srvrToLocal = new();

        #endregion Private Fields

        #region Public Constructors

        public IssueViewModel()
        {
            OpenSubIssueListCommand = new SharpBIMCommand(async (x) => await OpenSubIssueListAsync(x), "Load Sub Issues", Glyphs.empty, (x) => true);
            CloseIssueCommand = new SharpBIMCommand(async (x) => await CloseIssueAsync(x), "Close Issue", Glyphs.kpi_status_open, (x) => true);
            ShowOnWebCommand = new SharpBIMCommand(ShowOnWeb, "Show On Web", Glyphs.hyperlink_globe, (x) => true);
            PushIssueCommand = new SharpBIMCommand(async (x) => await PushIssueAsync(x), "Push", Glyphs.upload, (x) => true);
            ReloadIssueCommand = new SharpBIMCommand(async (x) => await ReloadIssue(x), "Reload", Glyphs.reload_sm, (x) => true);
            AddNewLableCommand = new SharpBIMCommand(AddNewLable, "Add New Label", Glyphs.plus_circle, (x) => true);
            AddLabelTextChangedCommand = new SharpBIMCommand(async (x) => await AddLabelTextChanged(x), "UpdateLabels", Glyphs.empty, (x) => true);
            LabelCollectionView = CollectionViewSource.GetDefaultView(AllLabelsList);
            OpenCommentsCommand = new SharpBIMCommand(OpenComments, "Open comments", Glyphs.comment, (x) => true);
            PastImageCommand = new SharpBIMCommand(PastImage, "Past Image", Glyphs.empty, (x) => true);
        }

        #endregion Public Constructors

        public SharpBIMCommand OpenCommentsCommand { get; set; }

        #region Public Properties

        public SharpBIMCommand AddNewLableCommand { get; set; }
        public SharpBIMCommand CloseIssueCommand { get; set; }

        public int Completed
        {
            get { return GetValue<int>(nameof(Completed)); }
            set { SetValue(value, nameof(Completed)); }
        }

        public string Description
        {
            get { return GetValue<string>(nameof(Description)); }
            set { SetValue(value, nameof(Description)); }
        }

        public SharpBIMCommand EditIssueCommand { get; set; }

        public bool IsClosed
        {
            get { return GetValue<bool>(nameof(IsClosed)); }
            set { SetValue(value, nameof(IsClosed)); }
        }

        public bool IsSubIssue => GetParentViewModel<IssueViewModel>() != null;

        public ObservableCollection<LabelModelView> IssueLables { get; set; } = [];
        public ObservableCollection<LabelModelView> AllLabelsList { get; set; } = [];
        public List<GitLabel> AllGitLabelsList { get; set; } = [];

        public string MarkDown
        {
            get { return GetValue<string>(nameof(MarkDown)); }
            set { SetValue(value, nameof(MarkDown)); }
        }

        public SharpBIMCommand OpenSubIssueListCommand { get; set; }

        public SharpBIMCommand PushIssueCommand { get; set; }

        public SharpBIMCommand ReloadIssueCommand { get; set; }

        public RepoModel SelectedRepo => GetParentViewModel<IssueListViewModel>().SelectedRepo;

        public bool ShowLabelsList
        {
            get { return GetValue<bool>(nameof(ShowLabelsList)); }
            set { SetValue(value, nameof(ShowLabelsList)); }
        }

        public SharpBIMCommand ShowOnWebCommand { get; set; }

        public int TotalSubIssues
        {
            get { return GetValue<int>(nameof(TotalSubIssues)); }
            set { SetValue(value, nameof(TotalSubIssues)); }
        }

        #endregion Public Properties

        #region Public Methods

        public void AddImage(string localUrl, ContentModel sithubUrl)
        {
            srvrToLocal.Add(localUrl, sithubUrl);
        }

        public void AddNewLable(object x)
        {
            try
            {
                //todo
                AppGlobals.MsgService.AlertUser(WindowHandle, "New Feature", "Feature not yet implemented");
            }
            catch (Exception ex)
            {
            }
        }

        public async Task CloseIssueAsync(object x)
        {
            try
            {
                IsClosed = !IsClosed;
                await PushIssueAsync(null);
            }
            catch (Exception ex)
            {
            }
        }

        public override void Init(IssueModel dataModel)
        {
            srvrToLocal.Clear();
            IssueLables.Clear();

            base.Init(dataModel);
            Title = dataModel.Title;
            Id = dataModel.number;
            Description = dataModel.body_text;

            IsClosed = dataModel.closed_at != null;
            if (dataModel.Id != -1)
            {
                Completed = dataModel.sub_issues_summary?.completed ?? 0;
                TotalSubIssues = dataModel.sub_issues_summary?.total ?? 0;
            }
        }

        public ICollectionView LabelCollectionView { get; set; }

        public SharpBIMCommand AddLabelTextChangedCommand { get; set; }

        // Add this line to the constructor

        public async Task AddLabelTextChanged(object x)
        {
            try
            {
                var value = (string)x;
                if (value.LastOrDefault() == '\t')
                {
                    value = value.Trim().ToLower();
                    if (value.Length > 1)
                    {
                        var exisitingLabel = AllLabelsList.FirstOrDefault(o => o.Title.EQ(value));
                        if (exisitingLabel != null)
                        {
                            var exlabelIssue = IssueLables.FirstOrDefault(o => o.Title.EQ(exisitingLabel.Title));
                            IssueLables.Remove(exlabelIssue);
                        }
                        else
                        {
                            var newLabel = new GitLabel() { name = value };

                            exisitingLabel = newLabel.ToModelView<LabelModelView>(this);
                            AllGitLabelsList.Add(newLabel);
                            AllLabelsList.Add(exisitingLabel);
                        }

                        IssueLables.Add(exisitingLabel);
                        TypedInLabelName = "";
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public string TypedInLabelName
        {
            get { return GetValue<string>(nameof(TypedInLabelName)); }
            set
            {
                if (value.Trim().Length == 0)
                    value = value.Trim();
                SetValue(value, nameof(TypedInLabelName));
                ShowLabelsList = value.Length > 0 && AllLabelsList.Any();
                LabelCollectionView.Filter = FilterLabels;
            }
        }

        private bool FilterLabels(object obj)
        {
            if (string.IsNullOrEmpty(TypedInLabelName))
                return true;
            if (TypedInLabelName == ".")
                return true;
            var labelmv = obj as LabelModelView;
            if (labelmv != null)
            {
                if (labelmv.ContextData.name.Contains(TypedInLabelName, StringComparison.OrdinalIgnoreCase))
                    return true;
                if (labelmv.ContextData.description.Contains(TypedInLabelName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public async Task LoadDetails()
        {
            if (IssueLables.Any())
                return;
            detailsLoaded = true;
            srvrToLocal.Clear();
            IssueLables.Clear();
            if (ContextData.labels != null)
            {
                foreach (var label in ContextData.labels)
                {
                    IssueLables.Add(label.ToModelView<LabelModelView>(this));
                }
            }

            if (!AllGitLabelsList.Any())
            {
                var repoLabelsReport = await LabelService.GetLables(SelectedRepo.name);
                if (!(repoLabelsReport.IsFailed))
                {
                    AllGitLabelsList = repoLabelsReport.Model.ToList();
                }
            }
            AllLabelsList.Clear();
            foreach (var label in AllGitLabelsList)
            {
                var labelmv = label.ToModelView<LabelModelView>(this);
                AllLabelsList.Add(labelmv);
            }

            Task.Run(async () => MarkDown = await ProcessImagesInMarkdownAsync());
        }

        public async Task<IServiceReport<IssueModel>> MakeSubIssue(bool force = false)
        {
            IssueModel parent = GetParentViewModel<IssueViewModel>().ContextData;
            IssueModel subIssue = this.ContextData;

            var report = await IssuesService.AddSubIssue(SelectedRepo.name, parent, subIssue, force);
            if (force == false && report.IsFailed)
            {
                var ans = AppGlobals.MsgService.AlertUser(WindowHandle, "Replace Parent", report.ErrorMessage, [Statics.REPLACE, Statics.CANCEL], SharpBIM.ServiceContracts.Enums.MessageType.Info);
                if (ans.Model == Statics.REPLACE)
                {
                    return await MakeSubIssue(true);
                }
            }

            if (force && report.IsFailed)
            {
                var ans = AppGlobals.MsgService.AlertUser(WindowHandle, "Replace Parent", report.ErrorMessage, [Statics.REPLACE, Statics.CANCEL], SharpBIM.ServiceContracts.Enums.MessageType.Info);
            }
            return report;
        }

        public async Task OpenSubIssueListAsync(object x)
        {
            try
            {
                var sublist = new SubIssueListViewModel() { ParentModelView = this };
                await sublist.LoadIssuesAsync(x);
                AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueListView), sublist);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task ReloadIssue(object x)
        {
            try
            {
                Children.Clear();
                IssueLables.Clear();
                AppGlobals.AppViewContext.UpdateProgress(1, 1, "Reloading issue", true);
                var reloadedReport = await IssuesService.GetIssue(SelectedRepo.name, ContextData.number);
                if (!reloadedReport.IsFailed)
                {
                    Init(reloadedReport.Model);
                }
                else
                    throw new Exception(reloadedReport.ErrorMessage);
            }
            catch (Exception ex)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Failed To Reload", ex.Message);
            }
            finally
            {
                AppGlobals.AppViewContext.UpdateProgress(1, 1, null, true);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private async Task<IServiceReport<IssueModel>> CreateIssueAsync()
        {
            IServiceReport<IssueModel> patchedReport = await IssuesService.CreateIssue(SelectedRepo.name, ContextData);
            if (!patchedReport.IsFailed)
            {
                ContextData = patchedReport.Model;
                if (!IsSubIssue)
                    await GetParentViewModel<IssueListViewModel>().AddItemsAsync([this], Token);
                else
                {
                    patchedReport = await MakeSubIssue();
                }
            }
            else
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Creation Failed", patchedReport.ErrorMessage);
                if (ParentModelView is IssueViewModel parentismv)
                {
                    parentismv.Dispatcher.Invoke(() => parentismv.Children.Remove(this));
                }
            }
            return patchedReport;
        }

        private async Task<string> ExtractImagesAsync(string markdownText)
        {
            string pattern = @"!\[.*?\]\((.*?)\)";
            var imageUrls = Regex.Matches(markdownText, pattern);
            foreach (Match match in imageUrls)
            {
                string imageUrl = match.Groups[1].Value;
                string imgId = imageUrl.Split('/').Last().Replace("?raw=true", "");

                var contentModel = (await ContentService.GetFile(GetParentViewModel<IssueListViewModel>().SelectedRepo.name, $"images/{imgId}"))?.Model;
                string localImagePath = null;
                if (contentModel != null)
                {
                    if (contentModel.type == "file" && contentModel.encoding == "base64")
                    {
                        var saveReport = await AppGlobals.FileService.SaveFile(new MemoryStream(Convert.FromBase64String(contentModel.content)), FileExtension.png);

                        if (saveReport.IsFailed)
                        {
                            AppGlobals.MsgService.AlertUser(WindowHandle, "Saving Image Failed", saveReport.ErrorMessage);
                        }
                        else
                        {
                            localImagePath = saveReport.Model;
                            contentModel.html_url = imageUrl;
                        }
                    }
                    else
                    {
                        CQC.Debug("another types not yet considered");
                    }
                }
                else
                {
                    var html = ContextData.body_html;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);
                    var imgNodes = doc.DocumentNode.SelectNodes("//img");
                    foreach (HtmlNode imgNode in imgNodes)
                    {
                        {
                            var imgUrl = imgNode.GetAttributeValue("src", "");
                            if (imgUrl.Contains(imgId))
                            {
                                localImagePath = await FetchImageAndSaveLocallyAsync(imgUrl);
                                contentModel = new ContentModel() { html_url = imgUrl };
                                break;
                            }
                        }
                    }
                }

                if (localImagePath != null)
                {
                    markdownText = markdownText.Replace(imageUrl, localImagePath);
                    AddImage(localImagePath, contentModel);
                }
            }
            return markdownText;
        }

        private async Task<string> FetchImageAndSaveLocallyAsync(string imageUrl)
        {
            string tempImagePath = null;
            using (var client = new HttpClient())
            {
                // Set the Authorization header with your token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AppGlobals.User.Token.access_token);

                // Make the request to fetch the image
                var response = await client.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Save the image locally (e.g., in a temp folder)
                    var stream = await response.Content.ReadAsStreamAsync();
                    var saveReport = await AppGlobals.FileService.SaveFile(stream, FileExtension.png);

                    if (saveReport.IsFailed)
                    {
                        AppGlobals.MsgService.AlertUser(WindowHandle, "Saving Image Failed", saveReport.ErrorMessage);
                    }
                    else
                        tempImagePath = saveReport.Model;
                }
            }

            return tempImagePath;
        }

        private async Task<IServiceReport<IssueModel>> PatchIssueAsync()
        {
            IServiceReport<IssueModel> patchedReport = await IssuesService.PatchIssue(SelectedRepo.name, ContextData);
            if (patchedReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Patch Failed", patchedReport.ErrorMessage);
                if (ParentModelView is IssueViewModel parentismv)
                {
                    parentismv.Dispatcher.Invoke(() => parentismv.Children.Remove(this));
                }
            }
            return patchedReport;
        }

        // returns body of the issue
        private async Task<IServiceReport<string>> PrepareForPushAsync()
        {
            var markdownreport = new ServiceReport<string>();

            string markDown = MarkDown;
            foreach (var key in srvrToLocal.Keys)
            {
                if (markDown.Contains(key))
                {
                    if (srvrToLocal[key] != null)
                    {
                        // keep image it is needed
                        markDown = markDown.Replace(key, srvrToLocal[key].html_url);
                    }
                    else
                    {
                        var finfo = new FileInfo(key);
                        finfo.MoveTo(Path.Combine(finfo.Directory.FullName, $"issue-{ContextData.number}-{DateTime.Now.Ticks}.png"));
                        var neName = finfo.FullName;
                        // this is a new image that needs uploading
                        var report = await IssuesService.UploadImageAsync(SelectedRepo.name, neName, ContextData.number, SelectedRepo.default_branch);
                        if (report.IsFailed)
                        {
                            markdownreport.Failed(report.ErrorMessage);
                        }
                        else
                        {
                            markDown = markDown.Replace(key, $"{report.Model}?raw=true");
                        }
                    }
                }
                else
                {
                    var contentModel = srvrToLocal[key];
                    if (contentModel.sha != null)
                    {
                        // img is deleted and to be removed from server, is it even possible... needs investigation?

                        var report = await ContentService.DeleteFile(SelectedRepo.name, srvrToLocal[key]);
                        if (report.IsFailed)
                        {
                            markdownreport.Failed(report.ErrorMessage);
                        }
                    }
                }
            }
            markdownreport.Model = markDown;

            return markdownreport;
        }

        public SharpBIMCommand PastImageCommand { get; set; }

        // Add this line to the constructor

        public void PastImage(object x)
        {
            try
            {
                AddImage(x.ToString(), null);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<string> ProcessImagesInMarkdownAsync()
        {
            string issueBody = ContextData.body?.ToString() ?? "";
            if (issueBody.Length > 0)
            {
                string markdownText = issueBody.Split('\\').LastOrDefault();
                markdownText = await ExtractImagesAsync(markdownText);

                return markdownText;
            }
            return issueBody;
        }

        // Add this line to the constructor

        public void OpenComments(object x)
        {
            try
            {
                var mv = new IssueCommentViewModel() { ParentModelView = this };
                mv.Init(ContextData);
                AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueCommentView), mv);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task PushIssueAsync(object obj)
        {
            AppGlobals.AppViewContext.UpdateProgress(0, 0, "Pushing", true);

            try
            {
                if (!VerifyIssueBoreISsue())
                {
                    return;
                }

                ContextData.Title = Title;

                IServiceReport<IssueModel> patchedReport = null;
                if (Id <= 0)
                {
                    patchedReport = await CreateIssueAsync();
                    if (patchedReport.IsFailed)
                    { return; }
                }

                var markdownReport = await PrepareForPushAsync();
                if (markdownReport.IsFailed)
                {
                    AppGlobals.MsgService.AlertUser(IntPtr.Zero, "Error", markdownReport.ErrorMessage);
                    return;
                }

                ContextData.body = markdownReport.Model;
                var orig = ContextData.state;
                ContextData.state = IsClosed ? IssueState.closed.ToString() : IssueState.open.ToString();
                ContextData.labels = IssueLables.Select(o => o.ContextData).ToArray();
                var newLabels = IssueLables.Where(o => o.Id == 0 || o.Description != o.ContextData.description);

                foreach (var label in newLabels)
                {
                    var labelModel = label.ContextData;
                    labelModel.description = label.Description;
                    if (labelModel.Id == 0)
                    {
                        await LabelService.CreateLabel(SelectedRepo.name, labelModel);
                    }
                    else
                    {
                        await LabelService.UpdateLabel(SelectedRepo.name, labelModel);
                    }
                }

                patchedReport = await PatchIssueAsync();

                if (patchedReport.IsFailed)
                {
                    return;
                }
                else
                {
                    Init(patchedReport.Model);
                    await LoadDetails();
                    if (orig != ContextData.state)
                    {
                        var val = ContextData.state == IssueState.closed.ToString() ? 1 : -1;
                        if (IsSubIssue)
                            GetParentViewModel<IssueViewModel>().Completed += val;
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                AppGlobals.AppViewContext.UpdateProgress(0, 0, null, true);
            }
        }

        private void ShowOnWeb(object obj)
        {
            IOEx.OpenUrl(ContextData.html_url);
        }

        private bool VerifyIssueBoreISsue()
        {
            if (string.IsNullOrEmpty(Title))
            {
                AppGlobals.MsgService.AlertUser(WindowHandle, "Missing something", "Issue must have a title");
                return false;
            }

            return true;
        }

        #endregion Private Methods
    }
}