using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SharpBIM.Utility;
using SharpBIM.WPF.Controls.UserControls;
using System.Windows.Ink;
using System.Windows.Media;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.GitTracker.Core.WPF.Helpers.MdPlugins;
using ICSharpCode.AvalonEdit.Highlighting;
using MdXaml.Plugins;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views
{
    using MdXaml;
    using ICSharpCode.AvalonEdit.Highlighting;
    using System.Windows.Media;
    using SharpBIM.WPF.Assets;
    using System.Collections.Generic;
    using MdXaml.Highlighting;
    using ICSharpCode.AvalonEdit.Document;
    using System.Windows.Documents;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Interaction logic for IssueView.xaml
    /// </summary>
    public partial class IssueView : SharpBIMUserControl
    {
        public IssueView()
        {
            InitializeComponent();

            avtxt.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("MarkDownWithFontSize");
            foreach (var cl in avtxt.SyntaxHighlighting.NamedHighlightingColors)
            {
                cl.Foreground = new SimpleHighlightingBrush(ResourceValues.SolidColorBrushs.ControlForegroundBrush.Color);
                cl.Background = new SimpleHighlightingBrush(ResourceValues.SolidColorBrushs.ControlBackground.Color);
            }

            mk.Plugins = MdXamlPlugins.Default;
            mk.Plugins.Inline.Add(new CheckboxInlineParser());
        }

        public new IssueViewModel ViewModel => DataContext as IssueViewModel;

        private void Avtxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel == null)
                return;
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
                    ViewModel.AddImage(tempPath, null);
                }
            }
        }
    }

    public class cusHi : IHighlightingDefinition
    {
        public string Name { get; }
        public HighlightingRuleSet MainRuleSet { get; }
        public IEnumerable<HighlightingColor> NamedHighlightingColors { get; }
        public IDictionary<string, string> Properties { get; }

        public HighlightingColor GetNamedColor(string name)
        {
            throw new NotImplementedException();
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            throw new NotImplementedException();
        }
    }
}