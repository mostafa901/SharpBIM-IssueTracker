global using System;

global using Task = System.Threading.Tasks.Task;
global using SharpBIM.UIContext.GitModels;

global using System.Threading.Tasks;
global using System.Linq;
global using System.Collections.Generic;

global using System.Collections.ObjectModel;
global using System.Text;

global using SharpBIM.IssueTracker.Core.GitHttp.Models;
global using SharpBIM.ServiceContracts.Interfaces;
global using SharpBIM.UIContext;
global using SharpBIM.Utility.Extensions;

global using System.IO;
global using System.Net.Http;
global using System.Net.Http.Headers;
global using System.Text.Json;

global using SharpBIM.IssueTracker.Core.Enums;
global using SharpBIM.ServiceContracts;
global using SharpBIM.ServiceContracts.Abstracts;


#if WINDOWS
global using System.Windows.Media;

global using SharpBIM.IssueTracker.Core.WPF.Mvvm.ViewModels;
global using SharpBIM.WPF.Assets;
global using SharpBIM.WPF.Assets.Fonts;

#endif
global using SharpBIM.IssueTracker.Core.GitHttp;
global using SharpBIM.Services;
global using SharpBIM.ServiceContracts.Interfaces.IIssueTrackers;
global using SharpBIM.Utility.Helpers;


global using static SharpBIM.IssueTracker.Core.IssueTrackerGlobals;

using Config = SharpBIM.Services.Config;


#if WINDOWS
using SharpBIM.WPF.Utilities;
#endif

namespace SharpBIM.IssueTracker.Core
{

    internal class IssueTrackerGlobals : Config
    {
  
        internal static IssueTrackerGlobals AppGlobals;
        //public string AppId { get; set; }
        //public string ClientSecret { get; set; }
        //public string PrivateKey { get; set; }
        //public string ClientId { get; set; }

        public static GitAuth AuthService { get; internal set; }
        public static GitRepos ReposSerivce { get; internal set; }
        public static GitIssues IssuesService { get; internal set; }
        public static GitContents ContentService { get; internal set; }
        public static GitAssignees GitAssigneesService { get; internal set; }
#if WINDOWS
        public static GitToken TokenService { get; internal set; }
#endif
        public static GitInstallation InstallService { get; internal set; }
        public static GitRelease ReleaseService { get; internal set; }
        public static GitLabels LabelService { get; internal set; }
        public static GitIssueComments CommentService { get; internal set; }
        internal IssueTrackerUser User => TrackerSettings.IssueTrackerUser;
        internal SharpBIMIssueTrackerSettings TrackerSettings   => AppSettings as SharpBIMIssueTrackerSettings;
        static IssueTrackerGlobals()
        {
            _ = new IssueTrackerGlobals();
        }

        public IssueTrackerGlobals()
        {
            AppGlobals = this;
#if WINDOWS
            var resource = new SharedResourceDictionary() { Source = new Uri("pack://application:,,,/SharpBIM.IssueTracker.Core;component/WPF/Mvvm/Views/DataTemplates.xaml") };
            SharpBIM.WPF.Globals.StyleResources = resource;
#endif
             AppSettings = SharpBIMSettings.Load<SharpBIMIssueTrackerSettings>(AppGlobals);
            SharpUser = TrackerSettings.IssueTrackerUser;
        }

        protected override void LoadSettings()
        {
            base.LoadSettings();
            ApplicationName = "SharpBIM.IssueTracker";
            ApplicationDisplayName = "SharpBIM-IssueTracker";
        }

        protected override void LoadServices()
        {
            base.LoadServices();
            AuthService = new(this);
            ReposSerivce = new(this);
            IssuesService = new(this);
            GitAssigneesService = new(this);
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

    internal class SharpBIMIssueTrackerSettings : SharpBIMSettings
    {

        public IssueTrackerUser IssueTrackerUser { get; set; }

        public SharpBIMIssueTrackerSettings()
        {
            IssueTrackerUser = new IssueTrackerUser();
        }
    }
}