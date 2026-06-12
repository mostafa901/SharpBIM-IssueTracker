// See https://aka.ms/new-console-template for more information

using SharpBIM.IssueTracker.Console;
using SharpBIM.IssueTracker.Core.GitHttp.Models;
using SharpBIM.UIContext.GitModels;
using SharpBIM.Utility.Extensions;

using System.Text.RegularExpressions;

using static SharpBIM.IssueTracker.Console.IssueTrackerConsolGlobals;

var AppGlobals = new IssueTrackerConsolGlobals();


var token = Environment.GetEnvironmentVariable("GitKey");

await AuthService.LoadGitConfigAsync();
//ReposSerivce.UpdateOwnerAccount("mostafa901");
var result = await AuthService.LoginByPersonalToken(token);

Console.WriteLine(result.ErrorMessage);



var repos = await ReposSerivce.GetRepos();

var trackerRepo = repos.Model.FirstOrDefault(o => o.name == "SharpBIM-IssueTracker");
#if false
var releases = await ReleaseService.GetReleasesByTag(trackerRepo, string.Empty);
foreach (var release in releases.Model)
{
    var tagtitle = release.name.Split('-')[0];
    tagtitle= $"{tagtitle.Trim()}" + " - " + $"{release.created_at:MMMM} {release.created_at.Year}";
    Console.WriteLine($"Updating release: {release.name} to {tagtitle}");
    release.name = tagtitle;
    await ReleaseService.UpdateRelease(trackerRepo, release);
}
Environment.Exit(0); 
#endif
var releasePath = "D:\\RevitApi\\Shared\\Study\\SharpBim.IssueTracker\\SharpBim.IssueTracker\\Release_Notes.md";
var releseNoteText = File.ReadAllText(releasePath);
string releaseNote = "##" + releseNoteText.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries)[1];

var reg = new Regex(" Version=\"(\\d+\\.\\d+\\.\\d+\\.\\d+)\"");
var matches = reg.Match(File.ReadAllText(@"D:\RevitApi\Shared\Study\SharpBim.IssueTracker\SharpBim.IssueTracker\source.extension.vsixmanifest"));
var match = matches.Groups[0].Value.Split('=')[1].Replace("\"", "");

string tag = "";
string title = "";
using (var str = new StringReader(releaseNote))
{
    while (true)
    {
        string line = str.ReadLine();
        if (line != null && line.Trim().Length > 2)
        {
            tag = line.Split('-')[0].Split(' ')[2];
            title = line;
            break;
        }
    }
}

if (tag != match)
{
    var updated = title.Replace(tag, tag = match);
    updated = updated.Split('-')[0].Trim() + " - " + $"{DateTime.Now:MMMM} {DateTime.Now.Year}";
    var updatedreleaseNote = releaseNote.Replace(title, updated);
    releseNoteText = releseNoteText.Replace(releaseNote, updatedreleaseNote);
    title = updated;
    File.WriteAllText(releasePath, releseNoteText);
    releaseNote = updatedreleaseNote.Replace(title, "");
}


var existingReleaseRepoert = await ReleaseService.GetReleasesByTag(trackerRepo, tag);

ReleaseModel releaseModel = null;
if (!existingReleaseRepoert.IsFailed)
{
    releaseModel = existingReleaseRepoert.Model.FirstOrDefault();
}
if (releaseModel == null)
{
    releaseModel = new ReleaseModel();
}

releaseModel.name = title.Replace("## ", "");
releaseModel.body = releaseNote + $"\r\n\r\n ## Download  \r\n [Market Place](https://marketplace.visualstudio.com/items?itemName=SharpBIM.SharpBIMGitTracker) \r\n [DirectDownload](https://marketplace.visualstudio.com/_apis/public/gallery/publishers/SharpBIM/vsextensions/SharpBIMGitTracker/{tag}/vspackage)";
if (releaseModel.id == 0)
{
    releaseModel.draft = false;
    releaseModel.tag_name = tag;
    var createReport = await ReleaseService.CreateRelease(trackerRepo, releaseModel);
    releaseModel = createReport.Model.First();
}
else
{
    var updatedReport = await ReleaseService.UpdateRelease(trackerRepo, releaseModel);
    releaseModel = updatedReport.Model.First();
}

var getAssetsReport = await ReleaseService.GetAssets(trackerRepo, releaseModel);

;