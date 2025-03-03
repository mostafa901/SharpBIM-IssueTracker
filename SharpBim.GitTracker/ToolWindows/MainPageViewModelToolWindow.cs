using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Imaging;
using SharpBIM.GitTracker.Core.WPF.Views;

namespace SharpBIM.GitTracker.ToolWindows
{
    [Obfuscation(Exclude = true, ApplyToMembers = true)]
    public class MainPageViewModelToolWindow : BaseToolWindow<MainPageViewModelToolWindow>
    {
        #region Public Properties

        public override Type PaneType => typeof(Pane);

        #endregion Public Properties

        #region Public Methods

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(
                new MainPage()
                );
        }

        public override string GetTitle(int toolWindowId) => "Issue Tracker";

        #endregion Public Methods

        #region Internal Classes

        [Guid("51b4cd6b-a823-40a9-ab54-db0e7c5ed22b")]
        internal class Pane : ToolkitToolWindowPane
        {
            #region Public Constructors

            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }

            #endregion Public Constructors
        }

        #endregion Internal Classes
    }
}