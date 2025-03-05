using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using SharpBim.GitTracker;
using SharpBIM.GitTracker.ToolWindows;

[assembly: InternalsVisibleTo("SharpBIM.GitTracker.Console", AllInternalsVisible = true)]

namespace SharpBIM.GitTracker
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("SharpBIM Git Tracker", "A tool for tracking and managing Git issues within Visual Studio", "1.0.0")]
    [ProvideToolWindow(typeof(MainPageViewModelToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.SharpBIMGitTrackerPackageString)]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public sealed class SharpBIMGitTrackerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.RegisterCommandsAsync();
            this.RegisterToolWindows();
        }
    }
}