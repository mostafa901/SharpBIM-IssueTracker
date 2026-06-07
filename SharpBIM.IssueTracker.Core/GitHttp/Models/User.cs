using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using SharpBIM.IssueTracker.Core.Auth;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.Services;
using SharpBIM.Utility.Extensions;

namespace SharpBIM.IssueTracker.Core.GitHttp.Models
{
    internal class IssueTrackerUser : SharpUserModel
    {
        public bool IsPersonalToken { get; set; }
        public InstallationModel Installation { get; set; }
        public Account UserAccount { get; set; }
        public bool IsOrg { get; set; } = false;
        public IssueTrackerUser()
        {
            Token = new SharpToken();
            UserSecret = "Sharpbim@gmail.com";
        }

        public string LastRepoName { get; set; }
        public string RepoOwner { get; set; }
        public bool LoggedIn { get; set; }

        [JsonIgnore]
        public override string Name { get => UserAccount?.login ?? ""; set => UserAccount?.login = value; }



    }
}