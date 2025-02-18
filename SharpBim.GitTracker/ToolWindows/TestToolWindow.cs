using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Imaging;
using SharpBim.GitTracker.Mvvm;

namespace SharpBim.GitTracker.ToolWindows
{
    public class TestToolWindow : BaseToolWindow<TestToolWindow>
    {
        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new TestView());
        }

        public override string GetTitle(int toolWindowId) => "SharpBIM Issue Tracker";

        [Guid("51b4cd6b-a823-40a9-ab54-db0e7c5ed22b")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}