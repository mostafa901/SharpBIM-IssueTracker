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
using SharpBim.GitTracker.Mvvm.ViewModels;
using SharpBIM.WPF.Controls.UserControls;

namespace SharpBim.GitTracker.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : SharpBIMUserControl
    {
        public LoginView()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
            pasBox.Password = AuthService.User?.Token?.access_token;
        }

        internal LoginViewModel ViewModel => DataContext as LoginViewModel;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModel.StoredToken = pasBox.Password;
        }
    }
}