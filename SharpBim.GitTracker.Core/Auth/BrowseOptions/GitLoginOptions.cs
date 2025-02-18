using SharpBIM.Utility.Extensions;
using System.Diagnostics;
using System.Net;

namespace SharpBim.GitTracker.Auth.BrowseOptions
{
    public class GitLoginOptions : GitBrowserOptions
    {
        private static string _state = StringEx.RandomString(6);

        public GitLoginOptions()
            : base($"https://github.com/login/oauth/authorize?client_id={AppGlobals.Config.ClientId}&state={_state}")
        {
        }

        public string Code { get; private set; }

        public override bool Validate(HttpListenerContext listener)
        {
            var query = listener.Request.QueryString;
            Code = query.Get(QueryString.CODE);

            if (Code != null && query[QueryString.STATE] == _state)
            {
                return true;
            }
            return false;
        }
    }
}