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
using SharpBIM.IssueTracker.Core.WPF.Mvvm.ViewModels;

namespace SharpBIM.IssueTracker.Core.WPF.Mvvm.Views
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

        protected override async Task<bool> OnLoadedAsync()
        {
            if (ViewModel.SelectedRepo == null)
            {
                await ViewModel.ReloadRepos(null);
                //ViewModel.ColView.Refresh();
            }
            return await base.OnLoadedAsync();
        }
        private IssueListViewModel ViewModel => DataContext as IssueListViewModel;

        private async void UiComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await ViewModel.LoadIssuesAsync(null);
        }
    }
}
