using SharpBIM.GitTracker.Auth;
using SharpBIM.Utility.Extensions;
using SharpBIM.GitTracker.Core.Properties;
using System.CodeDom;
using System.Threading.Tasks;
using SharpBIM.GitTracker.Core.Enums;

namespace SharpBIM.GitTracker
{
    public static class SharpGitTracker
    {
        /// <summary>
        /// this will Authorize the tool and login the user to his Repo
        /// </summary>
        /// <param name="userjson">can be null</param>
        /// <returns></returns>
        public static async Task<bool> Login()
        {
            var userjson = Settings.Default.USERJSON;
            AuthService ??= new GitAuth();
            AuthService.LoadUser(userjson);
            if (await AuthService.Login())
            {
                Settings.Default.USERJSON = AppGlobals.user.JSerialize();
                Settings.Default.Save();
                return true;
            }

            return false;
        }
    }
}