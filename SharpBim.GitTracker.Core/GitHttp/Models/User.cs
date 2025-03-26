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

        public GitUser()
        {
            Token = new SharpToken();
            var json = File.ReadAllText("appsettings.local.json");
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            MySecret = config["MySecret"];
        }

        private static string UserConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppGlobals.CompanyName, "GitTrackerConfig.json");

        public string LastRepoName { get; set; }
        public string MySecret { get; private set; }
        public string RepoOwner { get; set; }
        public bool LoggedIn { get; set; }

        public static GitUser? Parse()
        {
            var user = new GitUser();
            try
            {
                user = JsonSerializer.Deserialize<GitUser>(File.ReadAllText(UserConfigPath));
            }
            catch (Exception)
            {
            }

            return user;
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