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
using SharpBim.GitTracker.Auth;
using SharpBIM.Utility.Extensions;
using MessageBox = System.Windows.MessageBox;

namespace SharpBim.GitTracker.Mvvm
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
        }

        public TestView(bool contentLoaded)
        {
            _contentLoaded = contentLoaded;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AccessToken = "";
            var cl = new GitAuth(Properties.Settings.Default.AccessToken);

            if (await cl.Login())
            {
                Properties.Settings.Default.AccessToken = cl.user.JSerialize();
                Properties.Settings.Default.Save();
                Background = Brushes.DarkGreen;
            }
            else
                Background = Brushes.Red;
        }
    }
}