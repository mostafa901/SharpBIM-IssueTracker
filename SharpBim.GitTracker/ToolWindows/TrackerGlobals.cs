global using static SharpBim.GitTracker.ToolWindows.TrackerGlobals;
global using static SharpBIM.GitTracker.Core.GitTrackerGlobals;
using SharpBIM.Services;
using SharpBIM.WPF.Utilities;

namespace SharpBim.GitTracker.ToolWindows
{
    public class TrackerGlobals : Config
    {
        internal static TrackerGlobals AppGlobals;

        static TrackerGlobals()
        {
            AppGlobals = new TrackerGlobals();
            ResourceEx.ChangeTheme(true);
        }

        internal User User { get; set; }

        public TrackerGlobals()
        {
            User = User.Parse(Properties.Settings.Default.USERJSON);
        }
    }
}