global using static SharpBIM.GitTracker.TrackerGlobals;
using SharpBIM.GitTracker.Auth;
using SharpBIM.Services;
using SharpBIM.WPF.Utilities;

namespace SharpBIM.GitTracker
{
    public class TrackerGlobals : Config
    {
        internal static TrackerGlobals AppGlobals;

        static TrackerGlobals()
        {
            AppGlobals = new TrackerGlobals();
            ResourceEx.ChangeTheme(true);
        }

        public User User => SharpBIM.GitTracker.GitTrackerGlobals.AuthService.User;

        public TrackerGlobals()
        {
        }
    }
}