using System.Windows.Input;

using SharpBIM.WPF.Controls.UserControls;

namespace SharpBIM.IssueTracker.Core.WPF.Mvvm.Views
{
    using ICSharpCode.AvalonEdit.Highlighting;

    using System.Collections.Generic;
    //using MdXaml.Highlighting;

#if !NET472
    using Microsoft.VisualStudio.Shell;
#endif

    /// <summary>
    /// Interaction logic for IssueView.xaml
    /// </summary>
    public partial class IssueView : SharpBIMUserControl
    {
        public IssueView()
        {
            InitializeComponent();
            
        }

        public new IssueViewModel ViewModel => DataContext as IssueViewModel;

#if false
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
#endif

        private void labelTxtBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (lst.HasItems)
                {
                    lst.Focus();
                    lst.SelectedIndex = 0;
                }
            }

            if (!pop.IsOpen && labelTxtBox.Text.Length == 0 && e.Key == Key.Tab)
            {
                e.Handled = true;
                avtxt.Focus();
            }
            if (pop.IsOpen && e.Key == Key.Tab)
            {
                if (lst.HasItems)
                {
                    e.Handled = true;
                    lst.Focus();
                    lst.SelectedIndex = 0;
                }
            }
        }

        private void pop_Closed(object sender, EventArgs e)
        {
            labelTxtBox.Focus();
            labelTxtBox.SelectAll();
        }

        private void pop_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (pop.IsOpen && e.Key == Key.Back && e.OriginalSource is not TextBox)
            {
                e.Handled = true;
                labelTxtBox.Focus();
                labelTxtBox.SelectAll();
                return;
            }
            if (pop.IsOpen && e.Key == Key.Escape)
            {
                pop.IsOpen = false;
                labelTxtBox.Focus();
                labelTxtBox.SelectAll();
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