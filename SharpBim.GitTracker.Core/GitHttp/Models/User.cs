using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Services;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.GitTracker.Core.GitHttp.Models
{
    internal class GitUser : SharpUserModel
    {
        public bool IsPersonalToken { get; set; }
        public InstallationModel Installation { get; set; }
        public Account UserAccount { get; set; }
        public bool IsOrg { get; set; } = false;
        public GitUser()
        {
            Token = new SharpToken();
            ////string settingsPath = Path.Combine(AppGlobals.FileService.GetAssemblyDirectoryPath(), "appsettings.local.json");
            ////if (File.Exists(settingsPath))
            ////{
            ////    var json = File.ReadAllText(settingsPath);
            ////    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            ////    UserSecret  = config["UserSecret "];
            ////}
            UserSecret = "Sharpbim@gmail.com";
        }

        private static string UserConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SharpBIM", "GitTrackerConfig.json");

        public string LastRepoName { get; set; }
        public string RepoOwner { get; set; }
        public bool LoggedIn { get; set; }

        public static GitUser? Parse()
        {
            GitUser user = null;
            try
            {
                user = JsonSerializer.Deserialize<GitUser>(File.ReadAllText(UserConfigPath));
                if(user.IsPersonalToken)
                {
                    user.Name = "Personal";
                }
                else
                {
                    user.Name = "";
                }
            }
            catch (Exception)
            {
            }

            return user ??= new GitUser();
        }

        public void Save()
        {
            try
            {
                var json = this.JSerialize();
                File.WriteAllText(UserConfigPath, json);
            }
            catch (Exception)
            {
                // something went wronge
            }
        }
    }
}