using Microsoft.VisualStudio.Imaging;

using SharpBIM.IssueTracker.Core.WPF.Views;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SharpBIM.IssueTracker
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class SharpBIMToolWindow : BaseToolWindow<SharpBIMToolWindow>
    {
        public override string GetTitle(int toolWindowId) => "Issue Tracker";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new MainPage());
        }

        [Guid("880f7d86-f549-4fc4-8026-eb0548f1e833")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}