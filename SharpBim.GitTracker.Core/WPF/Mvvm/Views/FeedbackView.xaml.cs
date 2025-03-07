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
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for FeedbackView.xaml
    /// </summary>
    public partial class FeedbackView : SharpBIMUserControl
    {
        public FeedbackView()
        {
            InitializeComponent();
        }
    }

    public class FeedbackViewModel : ModelViewBase
    {
        public string IssueBody
        {
            get { return GetValue<string>(nameof(IssueBody)); }
            set { SetValue(value, nameof(IssueBody)); }
        }

        public string IssueTitle
        {
            get { return GetValue<string>(nameof(IssueTitle)); }
            set { SetValue(value, nameof(IssueTitle)); }
        }
    }
}