using IdentityModel.OidcClient.Browser;
using SharpBIM.ServiceContracts.Interfaces;
using System.Net;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.ServiceContracts.Interfaces.IGitTrackers;

namespace SharpBIM.GitTracker.Core.Auth.BrowseOptions
{
    public abstract class GitBrowserOptions : BrowserOptions
    {
        protected IGitConfig Config => AppGlobals.Config;

        protected GitBrowserOptions(string startUrl)
            : base(startUrl, "http://localhost:4567/github/callback")
        {
        }

        public virtual int TimeOut => 15; // 15 seconds

        public abstract bool Validate(HttpListenerContext listener);
    }
}