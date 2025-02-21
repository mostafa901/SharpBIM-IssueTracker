using IdentityModel.OidcClient.Browser;
using SharpBIM.ServiceContracts.Interfaces;
using System.Net;

namespace SharpBIM.GitTracker.Auth.BrowseOptions
{
    public abstract class GitBrowserOptions : BrowserOptions
    {
        protected IGitConfig Config => AppGlobals.Config;

        protected GitBrowserOptions(string startUrl)
            : base(startUrl, "http://localhost:4567/github/callback")
        {
        }

        public abstract bool Validate(HttpListenerContext listener);
    }
}