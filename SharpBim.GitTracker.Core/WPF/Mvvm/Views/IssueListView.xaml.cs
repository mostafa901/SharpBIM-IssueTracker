using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpBIM.WPF.Controls.UserControls;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for IssueListView.xaml
    /// </summary>
    public partial class IssueListView : SharpBIMUserControl
    {
        public IssueListView()
        {
            InitializeComponent();
        }

        private IssueListViewModel ViewModel => DataContext as IssueListViewModel;
    }
}
