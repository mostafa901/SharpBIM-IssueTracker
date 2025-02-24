using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Imaging;
using SharpBIM.GitTracker.Mvvm;

namespace SharpBIM.GitTracker.ToolWindows
{
    public class MainPageViewModelToolWindow : BaseToolWindow<MainPageViewModelToolWindow>
    {
        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new MainPage());
        }

        public override string GetTitle(int toolWindowId) => "SharpBIM";

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