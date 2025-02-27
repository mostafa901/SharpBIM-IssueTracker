using System.Linq;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using SharpBim.GitTracker;
using SharpBIM.GitTracker.ToolWindows;

namespace SharpBIM.GitTracker
{
    [Command(PackageIds.ShowTrackerWindowCommand)]
    internal sealed class ShowTrackerWindowCommand : BaseCommand<ShowTrackerWindowCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            await BaseToolWindow<MainPageViewModelToolWindow>.ShowAsync();
        }
    }
}