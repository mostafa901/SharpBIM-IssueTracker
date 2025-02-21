using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SharpBIM.WPF.Helpers.Commons;
using HtmlAgilityPack;
using SharpBIM.UIContexts;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.Utility.Extensions;
using System.Windows.Controls;
using SharpBIM.GitTracker.Mvvm.Views;
using System.Windows;
using SharpBim.GitTracker.ToolWindows;
using Microsoft;
using SharpBIM.ServiceContracts.QAQC;
using System.Text;

namespace SharpBIM.GitTracker.Mvvm.ViewModels
{
    public class IssueViewModel : ModelViewBase<IssueModel>
    {
        public override void Init(IssueModel dataModel)
        {
            srvrToLocal.Clear();
            base.Init(dataModel);
            Title = dataModel.Title;
            Id = dataModel.number;
            Description = dataModel.body_text;
            CloseIssueCommand = new SharpBIMCommand(CloseIssue, "Close Issue", Glyphs.kpi_status_open, (x) => true);
            ShowOnWebCommand = new SharpBIMCommand(ShowOnWeb, "Show On Web", Glyphs.hyperlink_open, (x) => true);
            EditIssueCommand = new SharpBIMCommand(EditIssue, "Edit", Glyphs.edit_tools, (x) => true);
            PushIssueCommand = new SharpBIMCommand(async (x) => await PushIssueAsync(x), "Push", Glyphs.upload, (x) => true);
            IsClosed = dataModel.closed_by != null;
            UpdateState();
        }

        private async Task PushIssueAsync(object obj)
        {
            ContextData.body = await PrepareForPushAsync();
            await IssuesService.PushIssue(ContextData);
            var updatedIssue = await IssuesService.GetIssue(this.GetParentViewModel<MainPageViewModel>().SelectedRepo, ContextData.number);
            Init(updatedIssue);
        }

        // returns body of the issue
        private async Task<string> PrepareForPushAsync()
        {
            string markDown = MarkDown;
            foreach (var key in srvrToLocal.Keys)
            {
                if (markDown.Contains(key))
                {
                    if (srvrToLocal[key].Length > 0)
                    {
                        // keep image it is needed
                        markDown = markDown.Replace(key, srvrToLocal[key]);
                    }
                    else
                    {
                        // this is a new image that needs uploading
                        var repo = this.GetParentViewModel<MainPageViewModel>().SelectedRepo;
                        var report = await IssuesService.UploadImageAsync(repo, key);
                        if (report.IsFailed)
                        {
                            AppGlobals.MsgService.AlertUser(IntPtr.Zero, "Error", report.ErrorMessage);
                        }
                        else
                        {
                            markDown = markDown.Replace(key, $"{report.Model}?raw=true");
                        }
                    }
                }
                else
                {
                    // img is deleted and to be removed from server, is it even possible... needs investigation?///
                }
            }
            return markDown;
        }

        private void EditIssue(object obj)
        {
            srvrToLocal.Clear();
            MainPage.Navigator.Navigate(new Page() { Content = new IssueView() { DataContext = this } });
            Task.Run(async () => MarkDown = await ProcessImagesInMarkdownAsync());
        }

        private void ShowOnWeb(object obj)
        {
            IOEx.OpenUrl(ContextData.html_url);
        }

        public string Description
        {
            get { return GetValue<string>(nameof(Description)); }
            set { SetValue(value, nameof(Description)); }
        }

        public string MarkDown
        {
            get { return GetValue<string>(nameof(MarkDown)); }
            set { SetValue(value, nameof(MarkDown)); }
        }

        private Dictionary<string, string> srvrToLocal = new();

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

        private async Task<string> ExtractImagesAsync(string markdownText)
        {
            string pattern = @"!\[.*?\]\((.*?)\)";
            var imageUrls = Regex.Matches(markdownText, pattern);
            foreach (Match match in imageUrls)
            {
                string imageUrl = match.Groups[1].Value;
                string imgId = imageUrl.Split('/').Last().Replace("?raw=true", "");

                var contentModel = (await ContentService.GetFile(GetParentViewModel<MainPageViewModel>().SelectedRepo, $"images/{imgId}"))?.Model;
                string localImagePath = null;
                if (contentModel != null)
                {
                    if (contentModel.type == "file" && contentModel.encoding == "base64")
                    {
                        localImagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), DateTime.Now.Ticks.ToString() + "image.png");
                        System.IO.File.WriteAllBytes(localImagePath, Convert.FromBase64String(contentModel.content));
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
                                break;
                            }
                        }
                    }
                }

                if (localImagePath != null)
                {
                    markdownText = markdownText.Replace(imageUrl, localImagePath);
                    AddImage(localImagePath, imageUrl);
                }
            }
            return markdownText;
        }

        public void AddImage(string localUrl, string sithubUrl)
        {
            srvrToLocal.Add(localUrl, sithubUrl);
        }

        private async Task<string> FetchImageAndSaveLocallyAsync(string imageUrl)
        {
            using (var client = new HttpClient())
            {
                // Set the Authorization header with your token
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SharpBIM.GitTracker.GitTrackerGlobals.AppGlobals.user.Token.access_token);

                // Make the request to fetch the image
                var response = await client.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    // Save the image locally (e.g., in a temp folder)
                    string tempImagePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "image.png");
                    var stream = await response.Content.ReadAsStreamAsync();
                    using (var fileStream = new System.IO.FileStream(tempImagePath, System.IO.FileMode.Create))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    return tempImagePath;
                }
            }

            return null;
        }

        public bool IsClosed
        {
            get { return GetValue<bool>(nameof(IsClosed)); }
            set { SetValue(value, nameof(IsClosed)); }
        }

        public SharpBIMCommand PushIssueCommand { get; set; }
        public SharpBIMCommand EditIssueCommand { get; set; }
        public SharpBIMCommand ShowOnWebCommand { get; set; }
        public SharpBIMCommand CloseIssueCommand { get; set; }

        public void CloseIssue(object x)
        {
            try
            {
                UpdateState();
                IsClosed = !IsClosed;
            }
            catch (Exception ex)
            {
            }
        }

        private void UpdateState()
        {
            if (IsClosed)
            {
            }
            else
            {
            }
        }
    }
}