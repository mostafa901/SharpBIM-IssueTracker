using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharpBim.GitTracker;
using SharpBIM.GitTracker;
using SharpBIM.GitTracker.ToolWindows;

namespace SharpBIM.GitTracker
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("SharpBIM Git Tracker", "A tool for tracking and managing Git issues within Visual Studio", "1.0.0")]
    [ProvideToolWindow(typeof(MainPageViewModelToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.SharpBIMGitTrackerString)]
    public sealed class SharpBIMGitTrackerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
        }
    }
}