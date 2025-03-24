using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MdXaml.Plugins;
using MdXaml;
using System.Windows.Documents;
using System.Windows.Media;
using SharpBIM.GitTracker.Core.WPF.Views;
using SharpBIM.WPF.Controls;

namespace SharpBIM.GitTracker.Core.WPF.Helpers.MdPlugins
{
    // modified to the original source:
    public class CheckboxInlineParser : IInlineParser
    {
        public CheckboxInlineParser()
        {
            this.FirstMatchPattern = new Regex(@"\[(?<value>[ |x])\]\s*(?<caption>[^\n|[]*)");
        }

        public Regex FirstMatchPattern { get; }

        public IEnumerable<Inline> Parse(string text, Match firstMatch, IMarkdown engine, out int parseTextBegin, out int parseTextEnd)
        {
            parseTextBegin = firstMatch.Index;
            parseTextEnd = firstMatch.Index + firstMatch.Length;

            UICheckBox chk = new()
            {
                IsChecked = "x".Equals(firstMatch.Groups["value"].Value, StringComparison.InvariantCultureIgnoreCase),
            };
            chk.GlyphSize = 8;
            chk.Loaded += Chk_Loaded;
            chk.Checked += (sender, e) => this.ReflectChkChangeInMarkdown(sender as UICheckBox, true, firstMatch.Value);
            chk.Unchecked += (sender, e) => this.ReflectChkChangeInMarkdown(sender as UICheckBox, false, firstMatch.Value);
            chk.Loaded += (sender, e) => this.UpdateChkEnabled(sender as UICheckBox, firstMatch.Value);

            if (firstMatch.Groups["caption"].Value is string caption && !string.IsNullOrEmpty(caption))
            {
                chk.Content = new FlowDocumentScrollViewer()
                {
                    Document = engine.Transform(caption),
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                    Focusable = false
                };
            }

            StackPanel sp = new();
            sp.Children.Add(chk);

            return new Inline[] { new InlineUIContainer(sp) };
        }

        private void Chk_Loaded(object sender, RoutedEventArgs e)
        {
            var chk = sender as UICheckBox;
            chk.Loaded -= Chk_Loaded;

            chk.Resources = Microsoft.VisualStudio.PlatformUI.ExtensionMethods.FindAncestor<MainPage>(chk)?.Resources;
        }

        private void UpdateChkEnabled(UICheckBox chk, string text)
        {
            if (this.FindParentMarkdownViewer(chk) is not MarkdownScrollViewer viewer)
            {
                chk.IsEnabled = false;
            }
            else
            {
                chk.IsEnabled = this.UniqueIndex(viewer.Markdown, text) >= 0;
            }
        }

        private void ReflectChkChangeInMarkdown(DependencyObject chk, bool isChecked, string text)
        {
            if (this.FindParentMarkdownViewer(chk) is not MarkdownScrollViewer viewer)
            {
                return;
            }

            string markdown = viewer.Markdown;
            int startIndex = this.UniqueIndex(markdown, text);
            if (startIndex == -1)
            {
                // Not unique
                return;
            }

            StringBuilder sb = new(viewer.Markdown);
            sb[startIndex + 1] = isChecked ? 'x' : ' ';
            viewer.Markdown = sb.ToString();
        }

        private MarkdownScrollViewer FindParentMarkdownViewer(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent is not MarkdownScrollViewer and not null)
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            if (parent is not MarkdownScrollViewer viewer)
            {
                return null;
            }

            return viewer;
        }

        private int UniqueIndex(string markdown, string text)
        {
            int firstIndex = markdown.IndexOf(text);
            if (firstIndex == -1)
            {
                // Not found
                return -1;
            }

            int secondIndex = markdown.IndexOf(text, firstIndex + 1);
            if (secondIndex != -1)
            {
                // Second hit
                return -1;
            }

            return firstIndex;
        }
    }
}