global using static SharpBim.GitTracker.GitTrackerGlobals;
using SharpBim.GitTracker.Auth;
using SharpBIM.Services;

namespace SharpBim.GitTracker
{
    public class GitTrackerGlobals : Config
    {
        public static GitTrackerGlobals AppGlobals;
        public string AppId { get; set; }
        public string ClientSecret { get; set; }
        public string PrivateKey { get; set; }
        public string ClientId { get; set; }

        public string UriAppName = "SharpBIM-IssueTracker";
        public IGitConfig Config { get; set; }
        public User? user { get; set; }

        static GitTrackerGlobals()
        {
            AppGlobals = new GitTrackerGlobals();
        }

        public GitTrackerGlobals()
        {
            ApplicationName = "SharpBIM.IssueTracker";
            ApplicationDisplayName = "SharpBIM IssueTracker";
            Config = GitConfig.Parse();
        }
    }
}