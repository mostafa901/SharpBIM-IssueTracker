global using System;

global using Task = System.Threading.Tasks.Task;
global using SharpBIM.UIContext.GitModels;

global using System.Threading.Tasks;
global using System.Linq;
global using System.Collections.Generic;

global using System.Collections.ObjectModel;
global using System.Text;

global using SharpBIM.GitTracker.Core.GitHttp.Models;
global using SharpBIM.ServiceContracts.Interfaces;
global using SharpBIM.UIContext;
global using SharpBIM.Utility.Extensions;

global using System.IO;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Text.Json;

global using SharpBIM.GitTracker.Core.Enums;
global using SharpBIM.GitTracker.Core.JsonConverters;
global using SharpBIM.ServiceContracts;
global using SharpBIM.ServiceContracts.Abstracts;


#if WINDOWS
global using System.Windows.Media;

global using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
global using SharpBIM.WPF.Assets;
global using SharpBIM.WPF.Assets.Fonts;

#endif
global using SharpBIM.GitTracker.Core.GitHttp;
global using SharpBIM.Services;
global using SharpBIM.ServiceContracts.Interfaces.IGitTrackers;
global using SharpBIM.Utility.Helpers;
global using SharpBIM.UIContext.Abstracts.Interfaces;


global using static SharpBIM.GitTracker.Core.GitTrackerGlobals;

using Config = SharpBIM.Services.Config;

#if WINDOWS
using SharpBIM.WPF.Utilities; 
#endif

using Microsoft.ServiceHub.Resources;
namespace SharpBIM.GitTracker.Core
{
    internal class GitTrackerGlobals : Config
    {
        internal static GitTrackerGlobals AppGlobals;
        //public string AppId { get; set; }
        //public string ClientSecret { get; set; }
        //public string PrivateKey { get; set; }
        //public string ClientId { get; set; }

         public static GitAuth AuthService { get; internal set; }
        public static GitRepos ReposSerivce { get; internal set; }
        public static GitIssues IssuesService { get; internal set; }
        public static GitContents ContentService { get; internal set; }
#if WINDOWS
        public static GitToken TokenService { get; internal set; } 
#endif
        public static GitInstallation InstallService { get; internal set; }
        public static GitRelease ReleaseService { get; internal set; }
        public static GitLabels LabelService { get; internal set; }
        public static GitIssueComments CommentService { get; internal set; }
        internal GitUser User { get => base.SharpUser as GitUser; set => base.SharpUser = value; }
        static GitTrackerGlobals()
        {
            _ = new GitTrackerGlobals();
        }

        public GitTrackerGlobals()
        {
            AppGlobals = this;
#if WINDOWS
            var resource = new SharedResourceDictionary() { Source = new Uri("pack://application:,,,/SharpBIM.GitTracker.Core;component/WPF/Mvvm/Views/DataTemplates.xaml") };
            SharpBIM.WPF.Globals.StyleResources.MergedDictionaries.Add(resource);
#endif
            User ??= GitUser.Parse();
            AuthService.LoadGitConfigAsync();
        }

        protected override void LoadServices()
        {
            base.LoadServices();
             ApplicationName = "SharpBIM.IssueTracker";
            ApplicationDisplayName = "SharpBIM-IssueTracker";
            AuthService = new(this);
            ReposSerivce = new(this);
            IssuesService = new(this);
            ContentService = new(this);
#if WINDOWS
            TokenService = new(this); 
#endif
            ReleaseService = new(this);
            LabelService = new(this);
            CommentService = new(this);
            InstallService = new(this);

        }
    }
}