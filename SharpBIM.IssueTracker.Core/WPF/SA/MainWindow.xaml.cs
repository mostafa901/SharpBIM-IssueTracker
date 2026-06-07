using SharpBIM.IssueTracker.Core.WPF.Views;
using SharpBIM.WPF.Controls;

namespace SharpBIM.IssueTracker.Core.WPF.SA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Content = new MainPage();
            UniqId = Guid.Parse("373f8439-dfc9-47cb-9ed6-625f734080eb");
        }
    }
}