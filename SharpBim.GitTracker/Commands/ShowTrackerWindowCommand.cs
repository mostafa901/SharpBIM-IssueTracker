using System.Reflection;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SharpBim.GitTracker;
using SharpBIM.GitTracker.ToolWindows;

namespace SharpBIM.GitTracker
{
    [Command(PackageGuids.ShowTrackerWindowCommandGuidString, PackageIds.ShowTrackerWindowCommand)]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    internal sealed class ShowTrackerWindowCommand : BaseCommand<ShowTrackerWindowCommand>
    {
        protected override async System.Threading.Tasks.Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await BaseToolWindow<MainPageViewModelToolWindow>.ShowAsync();
        }
    }
}