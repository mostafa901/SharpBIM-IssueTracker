using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SharpBim.GitTracker.Mvvm.ViewModels;
using SharpBim.GitTracker.ToolWindows;
using SharpBIM.ServiceContracts;
using SharpBIM.ServiceContracts.Enums;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts.QAQC;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBIM.GitTracker.Mvvm.ViewModels
{
    public class IssueViewModel : ModelViewBase<IssueModel>
    {
        #region Private Fields

        private Dictionary<string, ContentModel> srvrToLocal = new();

        #endregion Private Fields

        #region Public Properties

        public SharpBIMCommand CloseIssueCommand { get; set; }

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

        public string MarkDown
        {
            get { return GetValue<string>(nameof(MarkDown)); }
            set { SetValue(value, nameof(MarkDown)); }
        }

        public SharpBIMCommand PushIssueCommand { get; set; }

        public RepoModel SelectedRepo => GetParentViewModel<IssueListViewModel>().SelectedRepo;

        public SharpBIMCommand ShowOnWebCommand { get; set; }

        #endregion Public Properties

        #region Public Methods

        public void AddImage(string localUrl, ContentModel sithubUrl)
        {
            srvrToLocal.Add(localUrl, sithubUrl);
        }

        public void CloseIssue(object x)
        {
            try
            {
                IsClosed = !IsClosed;
            }
            catch (Exception ex)
            {
            }
        }

        public override void Init(IssueModel dataModel)
        {
            srvrToLocal.Clear();
            base.Init(dataModel);
            Title = dataModel.Title;
            Id = dataModel.number;
            Description = dataModel.body_text;
            CloseIssueCommand = new SharpBIMCommand(CloseIssue, "Close Issue", Glyphs.kpi_status_open, (x) => true);
            ShowOnWebCommand = new SharpBIMCommand(ShowOnWeb, "Show On Web", Glyphs.hyperlink_open, (x) => true);
            PushIssueCommand = new SharpBIMCommand(async (x) => await PushIssueAsync(x), "Push", Glyphs.upload, (x) => true);
            IsClosed = dataModel.closed_by != null;
        }

        #endregion Public Methods

        #region Private Methods

        private async Task<string> ExtractImagesAsync(string markdownText)
        {
            string pattern = @"!\[.*?\]\((.*?)\)";
            var imageUrls = Regex.Matches(markdownText, pattern);
            foreach (Match match in imageUrls)
            {
                string imageUrl = match.Groups[1].Value;
                string imgId = imageUrl.Split('/').Last().Replace("?raw=true", "");

                var contentModel = (await ContentService.GetFile(GetParentViewModel<IssueListViewModel>().SelectedRepo, $"images/{imgId}"))?.Model;
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
                        // this is a new image that needs uploading

                        var report = await IssuesService.UploadImageAsync(SelectedRepo, key);
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

                        var report = await ContentService.DeleteFile(SelectedRepo, srvrToLocal[key]);
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

        private async Task PushIssueAsync(object obj)
        {
            var markdownReport = await PrepareForPushAsync();
            ;
            if (markdownReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(IntPtr.Zero, "Error", markdownReport.ErrorMessage);
                return;
            }
            ContextData.body = markdownReport.Model;
            var patchedReport = await IssuesService.PushIssue(ContextData);
            if (patchedReport.IsFailed)
            {
                AppGlobals.MsgService.AlertUser(IntPtr.Zero, "Patch Failed", patchedReport.ErrorMessage);
            }
            else
            {
                var updatedIssue = await IssuesService.GetIssue(SelectedRepo, ContextData.number);
                Init(updatedIssue);
            }
        }

        private void ShowOnWeb(object obj)
        {
            IOEx.OpenUrl(ContextData.html_url);
        }

        private bool detailsLoaded;

        public void LoadDetails()
        {
            if (detailsLoaded)
                return;
            detailsLoaded = true;
            srvrToLocal.Clear();
            Task.Run(async () => MarkDown = await ProcessImagesInMarkdownAsync());
        }

        #endregion Private Methods
    }
}