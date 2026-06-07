global using Community.VisualStudio.Toolkit;

global using Microsoft.VisualStudio.Shell;

global using System;

global using Task = System.Threading.Tasks.Task;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SharpBIM.IssueTracker
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideToolWindow(typeof(SharpBIMToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.SharpBIMTrackerPackageString)]
    [Obfuscation(Exclude = true, ApplyToMembers = true)]

    public sealed class SharpBIMIssueTrackerPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await this.RegisterCommandsAsync();

        this.RegisterToolWindows();
    }
}
}