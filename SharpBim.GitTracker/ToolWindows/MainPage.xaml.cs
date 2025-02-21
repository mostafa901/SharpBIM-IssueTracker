global using static SharpBIM.GitTracker.GitTrackerGlobals;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using SharpBim.GitTracker.Mvvm.Views;
using SharpBim.GitTracker.ToolWindows;
using SharpBIM.GitTracker.Auth;
using SharpBIM.GitTracker.GitHttp;
using SharpBIM.GitTracker.Mvvm.Views;
using SharpBIM.ServiceContracts.QAQC;
using SharpBIM.Services;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Controls.UserControls;
using SharpBIM.WPF.Helpers.Extension;
using SharpBIM.WPF.Utilities;

namespace SharpBIM.GitTracker.Mvvm
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : SharpBIMUserControl
    {
        public static NavigationService Navigator { get; set; }

        public MainPage()
        {
            CQC.BreakOnUnObserved = false;
            var gp = new TrackerGlobals();
            ResourceEx.ChangeTheme(true);

            InitializeComponent();
            Navigator = SharpBIMViewer.NavigationService;
            DataContext = new MainPageViewModel();
            Loaded += TestView_Loaded;
        }

        private void TestView_Loaded(object sender, RoutedEventArgs e)
        {
            var v = new IssueListView() { DataContext = DataContext };
            SharpBIMViewer.Navigate(new Page { Content = v });
        }

        public MainPage(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }
    }

    public class TrackerGlobals : Config
    {
    }
}