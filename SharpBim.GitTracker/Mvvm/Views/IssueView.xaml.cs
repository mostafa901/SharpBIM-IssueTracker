using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SharpBIM.GitTracker.Mvvm.ViewModels;
using SharpBIM.Utility;
using SharpBIM.WPF.Controls.UserControls;
using System.Windows.Ink;
using System.Windows.Media;

namespace SharpBIM.GitTracker.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for IssueView.xaml
    /// </summary>
    public partial class IssueView : SharpBIMUserControl
    {
        public IssueView()
        {
            InitializeComponent();
            DataContext = new IssueViewModel();
            avtxt.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("MarkDownWithFontSize");
            mk.MarkdownStyle = MdXaml.MarkdownStyle.GithubLike;
        }

        public new IssueViewModel ViewModel => DataContext as IssueViewModel;

        private void Avtxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (Clipboard.ContainsImage())
                {
                    e.Handled = true;
                    var img = Clipboard.GetImage();
                    string tempPath = System.IO.Path.GetTempFileName() + ".jpg";
                    File.WriteAllBytes(tempPath, img.ToBytes(new JpegBitmapEncoder()));
                    string imageReference = $"![Image]({tempPath})";  // Markdown-style
                    avtxt.Text = avtxt.Text.Insert(avtxt.CaretOffset, imageReference);
                    ViewModel.AddImage(tempPath, string.Empty);
                }
            }
        }
    }
}