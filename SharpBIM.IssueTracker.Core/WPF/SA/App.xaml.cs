using System.Configuration;
using System.Data;
using System.Windows;

namespace SharpBIM.IssueTracker.Core.WPF.SA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppGlobals.PlatformType = SharpBIM.ServiceContracts.Enums.Platform.SA;
        }
    }

}
