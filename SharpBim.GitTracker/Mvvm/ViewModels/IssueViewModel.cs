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

namespace SharpBIM.GitTracker.Mvvm.ViewModels
{
    public class IssueViewModel : ModelViewBase<IssueModel>
    {
        public override void Init(IssueModel dataModel)
        {
            base.Init(dataModel);
            Title = dataModel.Title;
            Id = dataModel.number;
            Task.Run(async () => MarkDown = await ProcessImagesInMarkdown());
            Description = dataModel.body_text;
            CloseIssueCommand = new SharpBIMCommand(CloseIssue, "Close Issue", Glyphs.kpi_status_open, (x) => true);
            ShowOnWebCommand = new SharpBIMCommand(ShowOnWeb, "Show On Web", Glyphs.hyperlink_open, (x) => true);
            EditIssueCommand = new SharpBIMCommand(EditIssue, "Edit", Glyphs.edit_tools, (x) => true);
            PushIssueCommand = new SharpBIMCommand(PushIssue, "Push", Glyphs.upload, (x) => true);
            IsClosed = dataModel.closed_by != null;
            UpdateState();
 
        }

        private void PushIssue(object obj)
        {
             
        }

        private void EditIssue(object obj)
        {
            MainPage.Navigator.Navigate(new Page() { Content = new IssueView() { DataContext = this } });
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

        private async Task<string> ProcessImagesInMarkdown()
        {
            string issueBody = ContextData.body?.ToString() ?? "";
            if (issueBody.Length > 0)
            {
                string markdownText = issueBody.Split('\\').LastOrDefault();
                string html = ContextData.body_html;
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                markdownText = await ExtractImages(markdownText, doc);

                return markdownText;
            }
            return issueBody;
        }

        private async Task<string> ExtractImages(string markdownText, HtmlDocument doc)
        {
            var imgs = doc.DocumentNode.DescendantNodes().Where(n => n.Name == "img").ToList();

            var imageUrls = Regex.Matches(markdownText, @"!\[.*?\]\((.*?)\)");
            foreach (Match match in imageUrls)
            {
                string imageUrl = match.Groups[1].Value;
                string imgId = imageUrl.Split('/').Last();

                foreach (var img in imgs)
                {
                    var src = img.Attributes["src"].Value;
                    if (src.Contains(imgId))
                    {
                        var localImagePath = await FetchImageAndSaveLocally(src);
                        if (!string.IsNullOrEmpty(localImagePath))
                        {
                            // Replace the image URL with the local path
                            srvrToLocal.Add(imageUrl, localImagePath);
                            markdownText = markdownText.Replace(imageUrl, localImagePath);
                        }
                        // Find all image links in the Markdown text using a regular expression
                    }
                }
            }

            return markdownText;
        }

        private async Task<string> FetchImageAndSaveLocally(string imageUrl)
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