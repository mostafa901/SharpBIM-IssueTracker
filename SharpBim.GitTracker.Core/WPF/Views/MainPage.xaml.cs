global using System.Windows;
global using System.Windows.Controls;
global using System.Windows.Navigation;
using SharpBIM.ServiceContracts.Interfaces;
using SharpBIM.ServiceContracts.QAQC;
using SharpBIM.UIContexts;
using SharpBIM.Utility.Extensions;
using SharpBIM.WPF.Controls.UserControls;
using SharpBIM.WPF.Helpers.Commons;
using SharpBIM.WPF.Utilities;

namespace SharpBIM.GitTracker.Core.WPF.Views
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
            AppGlobals.AppViewContext = this;
            ResourceEx.ChangeTheme(true);
            CQC.BreakOnUnObserved = false;

            InitializeComponent();
            DataContext = new MainPageViewModel();
            Navigator = SharpBIMViewer.NavigationService;
            Loaded += MainPage_Loaded;
        }

        private new MainPageViewModel ViewModel => DataContext as MainPageViewModel;

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainPage_Loaded;

            ViewModel.WindowHandle = this.GetWindow().Handle();
            await ViewModel.Login(null);
        }

        public async void UpdateProgress(double value, double max, string message, bool isIndeterminate)
        {
            await this.Dispatcher.InvokeAsync(new Action(() =>
                  {
                      ViewModel.ShowProgressBar = !string.IsNullOrEmpty(message);
                      ViewModel.IsBusy = prgBar.Visibility == Visibility.Visible;
                      ViewModel.ProgressMessage = message;
                      System.Windows.Forms.Application.DoEvents();
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

        public Action<Page> AppNavigate { get; set; }
        private Stack<object> ContextHistory = [];

        public Action<Type, object> AppNavigateTo { get; set; } = (s, e) =>
            {
                var uc = Activator.CreateInstance(s) as FrameworkElement;
                var page = new Page() { Content = uc };

                ((FrameworkElement)page.Content).DataContext = e;

                ((ModelViewBase)(e)).GetParentViewModel<MainPageViewModel>().Title = ((ModelViewBase)e).Title;
                Navigator.Navigate(page);
            };

        public MainPage(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }
    }
}