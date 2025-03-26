using SharpBIM.Utility.Extensions;
using System.Net;
using SharpBIM.GitTracker.Core.Auth;
using SharpBIM.GitTracker.Core.Auth.BrowseOptions;

namespace SharpBIM.GitTracker.Core.Auth.BrowseOptions
{
    public class GitInstallOptions : GitBrowserOptions
    {
        private static string _state = StringEx.RandomString(6);

        public GitInstallOptions()
            : base($"https://github.com/apps/{Uri.EscapeDataString(AppGlobals.UriAppName)}/installations/new?state={_state}")
        {
        }

        public string Code { get; private set; }
        public string InstallationId { get; private set; }

        public override bool Validate(HttpListenerContext listener)
        {
            //response sample
            //code=ab8919ba2f4908db730f&installation_id=61256880&setup_action=install&state=BCQEDX
            var query = listener.Request.QueryString;
            Code = query.Get(QueryString.CODE);
            InstallationId = query.Get(QueryString.INSTALLATIONID);
            var action = query.Get(QueryString.SETUPACTION);

            if (Code != null && action == "install" && query[QueryString.STATE] == _state)
            {
                return true;
            }
            return false;
        }
    }
}