
using SharpBim.IssueTracker;

using System.Reflection;

using Community.VisualStudio.Toolkit;

using Microsoft.VisualStudio.Shell;

namespace SharpBIM.IssueTracker
{
    [Command(PackageGuids.SharpBIMTrackerCommandString, PackageIds.MyCommand)]

    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    internal sealed class SharpBIMToolWindowCommand : BaseCommand<SharpBIMToolWindowCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return SharpBIMToolWindow.ShowAsync();
        }
    }
}
