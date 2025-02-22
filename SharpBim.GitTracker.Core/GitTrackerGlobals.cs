global using static SharpBIM.GitTracker.GitTrackerGlobals;
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.Core.GitHttp;
using SharpBIM.GitTracker.GitHttp;
using SharpBIM.Services;

namespace SharpBIM.GitTracker
{
    public class GitTrackerGlobals : Config
    {
        internal static GitTrackerGlobals AppGlobals;
        public string AppId { get; set; }
        public string ClientSecret { get; set; }
        public string PrivateKey { get; set; }
        public string ClientId { get; set; }

        public string UriAppName = "SharpBIM-IssueTracker";
        public IGitConfig Config { get; set; }
        public User? user { get; set; }
        public static GitAuth AuthService { get; internal set; }
        public static GitRepos ReposSerivce { get; internal set; }
        public static GitIssues IssuesService { get; internal set; }
        public static GitContents ContentService { get; internal set; }
        public static GitToken TokenService { get; internal set; }
        public static GitInstallation InstallService { get; internal set; }

        static GitTrackerGlobals()
        {
            AppGlobals = new GitTrackerGlobals();
        }

        public GitTrackerGlobals()
        {
            ApplicationName = "SharpBIM.IssueTracker";
            ApplicationDisplayName = "SharpBIM IssueTracker";
            Config = GitConfig.Parse();
            AuthService = new();
            ReposSerivce = new();
            IssuesService = new();
            ContentService = new();
            TokenService = new();
            InstallService = new();
        }
    }
}