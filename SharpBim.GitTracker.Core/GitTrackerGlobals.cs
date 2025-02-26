﻿global using static SharpBIM.GitTracker.Core.GitTrackerGlobals;
using SharpBIM.GitTracker.Core.GitHttp;
using SharpBIM.GitTracker.GitHttp;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core;
using SharpBIM.GitTracker;
using SharpBIM.Services;
using SharpBIM.ServiceContracts.Interfaces.IGitTrackers;
using System.Threading.Tasks;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts;

namespace SharpBIM.GitTracker.Core
{
    public class GitTrackerGlobals : Config
    {
        internal static GitTrackerGlobals AppGlobals;
        public string AppId { get; set; }
        public string ClientSecret { get; set; }
        public string PrivateKey { get; set; }
        public string ClientId { get; set; }

        public string UriAppName = "SharpBIM-IssueTracker";
        internal IGitConfig Config { get; set; }
        internal IUser? User { get; set; }
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
            AuthService = new();
            ReposSerivce = new();
            IssuesService = new();
            ContentService = new();
            TokenService = new();
        }
    }
}