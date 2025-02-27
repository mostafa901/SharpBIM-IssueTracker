﻿using System.IO;
using System.Text.Json;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.GitTracker.Core.GitHttp.Models
{
    internal class GitUser : IUser
    {
        public bool IsPersonalToken { get; set; }
        public UserToken Token { get; set; }
        public InstallationModel Installation { get; set; }
        public Account UserAccount { get; set; }

        public GitUser()
        {
            Token = new UserToken();
        }

        private static string UserConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppGlobals.CompanyName, "GitTrackerConfig.json");

        public string LastRepoName { get; set; }
        public string Email { get; set; }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Version { get; set; }
        public string HashedPassword { get; set; }
        IUserToken ISharpUser.Token { get; set; }
        public bool LoggedIn { get; set; }

        public static GitUser? Parse()
        {
            try
            {
                return JsonSerializer.Deserialize<GitUser>(File.ReadAllText(UserConfigPath));
            }
            catch (Exception)
            {
                return new GitUser();
            }
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