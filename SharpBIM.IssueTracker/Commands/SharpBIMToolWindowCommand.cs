
using System.Reflection;

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
