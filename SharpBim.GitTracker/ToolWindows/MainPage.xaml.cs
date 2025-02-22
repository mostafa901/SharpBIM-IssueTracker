global using static SharpBIM.GitTracker.GitTrackerGlobals;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SharpBim.GitTracker.Mvvm.ViewModels;
using SharpBim.GitTracker.Mvvm.Views;
using SharpBim.GitTracker.ToolWindows;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts.QAQC;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Controls.UserControls;

namespace SharpBIM.GitTracker.Mvvm
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : SharpBIMUserControl, IAppViewModel
    {
        public static NavigationService Navigator { get; set; }
        public Action GoBack { get; set; }
        public Action<object> OnDocumentChanged { get; set; }
        public Action<object> OnIdling { get; set; }
        public Action UpdateConsole { get; set; }
        public ObservableCollection<RichTextBox> ConsoleMessages { get; set; }
        public SharpBIMCommand ExecuteConsoleCommand { get; set; }
        public string ConsoleInput { get; set; }

        public MainPage()
        {
            CQC.BreakOnUnObserved = false;
            AppGlobals.AppViewContext = this;

            InitializeComponent();
            DataContext = new MainPageViewModel();
            Navigator = SharpBIMViewer.NavigationService;
        }

        private void ViewModel_LoggedIn(object sender, EventArgs e)
        {
            AppGlobals.AppViewContext.AppNavigateTo(typeof(IssueListView), new IssueListViewModel() { ParentModelView = ViewModel });
        }

        protected override async Task<bool> OnLoadedAsync()
        {
            if ((await AuthService.Login()).IsFailed)
            {
                var loginView = new LoginView();
                loginView.ViewModel.LoggedIn += ViewModel_LoggedIn;
                SharpBIMViewer.Navigate(loginView);
                if (SharpBIMViewer.CanGoBack)
                {
                    SharpBIMViewer.RemoveBackEntry();
                }
            }
            else
            {
                ViewModel_LoggedIn(null, null);
            }
            ViewModel.WindowHandle = this.GetWindow().Handle();
            return true;
        }

        public void UpdateProgress(double value, double max, string message, bool isIndeterminate)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                prgBar.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
                prgBarTxt.Text = message;
                if (isIndeterminate)
                {
                    prgBar.IsIndeterminate = true;
                }
                else
                {
                    prgBar.IsIndeterminate = false;
                    prgBar.Minimum = value;
                    prgBar.Maximum = max;
                }
                    System.Windows.Forms.Application.DoEvents();
            }));
        }

        public void RemoveUpdater(Guid id)
        {
            throw new NotImplementedException();
        }

        public void WriteToConsole(object message)
        {
            throw new NotImplementedException();
        }

        public void WriteToFile(object message)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<Type, Page> ViewDic = [];
        public Action<Page> AppNavigate { get; set; }

        public Action<Type, object> AppNavigateTo { get; set; } = (s, e) =>
            {
                ViewDic.TryGetValue(s, out Page page);
                if (page == null)
                {
                    var uc = Activator.CreateInstance(s) as FrameworkElement;
                    page = new Page() { Content = uc };
                    ViewDic.Add(s, page);
                }

                if (((FrameworkElement)page.Content).DataContext != e)
                    ((FrameworkElement)page.Content).DataContext = e;
                Navigator.Navigate(page);
            };

        public MainPage(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }
    }
}