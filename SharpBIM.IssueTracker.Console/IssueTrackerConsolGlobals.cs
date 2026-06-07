using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpBIM.IssueTracker.Core;
using SharpBIM.IssueTracker.Core.GitHttp;
using SharpBIM.Services;

namespace SharpBIM.IssueTracker.Console
{
    internal class IssueTrackerConsolGlobals : Config
    {
        public static GitAuth AuthService { get; private set; }
        public static GitRepos ReposSerivce { get; private set; }
        public static GitIssues IssueService { get; private set; }
        public static GitContents ContentService { get; private set; }
        public static GitRelease ReleaseService { get; private set; }
        public static GitLabels LabelService { get; private set; }
        public static GitIssueComments CommentService { get; private set; }
        public static GitInstallation InstallService { get; private set; }

        protected override void LoadSettings()
        {
            base.LoadSettings();
            SharpUser = new SharpUserModel();
        }
        protected override void LoadServices()
        {
            base.LoadServices();
            var accessToken = Environment.GetEnvironmentVariable("GitPSToken");
            GitService = new SharpGitService(accessToken, this);
            ApplicationName = "SharpBIM.GitTracker";
            ApplicationDisplayName = "SharpBIM-GitTracker";
            AuthService = new(this);
            ReposSerivce = new(this);
            IssueService = new(this);
            ContentService = new(this);
            ReleaseService = new(this);
            LabelService = new(this);
            CommentService = new(this);
            InstallService = new(this);

        }
    }
}