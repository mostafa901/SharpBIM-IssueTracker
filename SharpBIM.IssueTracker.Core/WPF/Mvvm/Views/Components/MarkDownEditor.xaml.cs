

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;

using SharpBIM.WPF.Controls;

using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SharpBIM.IssueTracker.Core.WPF.Mvvm.Views.Components
{
    /// <summary>
    /// Interaction logic for MarkDownEditor.xaml
    /// </summary>
    public partial class MarkDownEditor : TextEditor
    {
        #region ImagePastedCommand

        public static readonly DependencyProperty ImagePastedCommandProperty = DependencyProperty.Register(
        nameof(ImagePastedCommand),
        typeof(SharpBIMCommand),
        typeof(MarkDownEditor),
        new PropertyMetadata(null, OnImagePastedCommandChanged)
    );
        private SpellCheckContextMenuProvider rprov;

        private static void OnImagePastedCommandChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
        )
        {
            // DO SOMETHING
        }

        public SharpBIMCommand ImagePastedCommand
        {
            get { return (SharpBIMCommand)GetValue(ImagePastedCommandProperty); }
            set { SetValue(ImagePastedCommandProperty, value); }
        }

        #endregion ImagePastedCommand

        public MarkDownEditor()
        {
            InitializeComponent();
            SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("MarkDownWithFontSize");
            foreach (var cl in SyntaxHighlighting.NamedHighlightingColors)
            {
                cl.Foreground = new SimpleHighlightingBrush(ResourceValues.SolidColorBrushs.ControlForegroundBrush.Color);
                cl.Background = new SimpleHighlightingBrush(ResourceValues.SolidColorBrushs.ControlBackground.Color);
            }
            Loaded += MarkDownEditor_Loaded;
            Unloaded += MarkDownEditor_Unloaded;
        }

        private void MarkDownEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            rprov.Dispose();
        }

        private void MarkDownEditor_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MarkDownEditor_Loaded;
            rprov = new SpellCheckContextMenuProvider(this);
            TextArea.TextView.LineTransformers.Add(new SpellCheckColorizer());
        }

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
                    Text = Text.Insert(CaretOffset, imageReference);
                    ImagePastedCommand.Execute(tempPath);
                }
            }
        }
    }


    public class SpellCheckContextMenuProvider : IDisposable
    {
        private readonly TextEditor _textEditor;
        private readonly TextBox _hiddenTextBox;

        public SpellCheckContextMenuProvider(TextEditor textEditor)
        {
            _textEditor = textEditor;
            _textEditor.TextArea.ContextMenu = new UiContextMenu();

            _hiddenTextBox = new TextBox
            {
                SpellCheck = { IsEnabled = true },
                Visibility = Visibility.Hidden
            };

            _textEditor.TextArea.ContextMenuOpening += OnContextMenuOpening;
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var textArea = _textEditor.TextArea;
            var position = textArea.TextView.GetPositionFloor(
                Mouse.GetPosition(textArea.TextView) + textArea.TextView.ScrollOffset);

            if (!position.HasValue)
                return;

            int offset = _textEditor.Document.GetOffset(position.Value.Location);
            var wordInfo = GetWordAtOffset(offset);

            if (wordInfo == null)
                return;

            // Check for spelling error
            var line = _textEditor.Document.GetLineByOffset(offset);
            _hiddenTextBox.Text = _textEditor.Document.GetText(line);
            int posInLine = offset - line.Offset;

            SpellingError error = _hiddenTextBox.GetSpellingError(posInLine);
            if (error == null)
                return;

            // Create or modify existing context menu
            if (textArea.ContextMenu == null)
            {
                textArea.ContextMenu = new UiContextMenu();
            }

            var contextMenu = textArea.ContextMenu;
            contextMenu.Items.Clear();

            // Add suggestions
            int suggestionCount = 0;
            foreach (string suggestion in error.Suggestions.Take(5))
            {
                var item = new UiMenuItem
                {
                    Header = suggestion,
                    FontWeight = FontWeights.Bold
                };

                string suggestionCopy = suggestion; // Capture for closure
                int wordOffset = wordInfo.Value.Offset;
                int wordLength = wordInfo.Value.Length;

                item.Click += (s, args) =>
                {
                    _textEditor.Document.Replace(wordOffset, wordLength, suggestionCopy);
                };

                contextMenu.Items.Add(item);
                suggestionCount++;
            }

            if (suggestionCount > 0)
                contextMenu.Items.Add(new Separator());

            // Ignore All
            var ignoreItem = new MenuItem { Header = "Ignore All" };
            string word = wordInfo.Value.Word;
            ignoreItem.Click += (s, args) =>
            {
                //_hiddenTextBox.SpellCheck.IgnoreAll(word);
                _textEditor.TextArea.TextView.Redraw();
            };
            contextMenu.Items.Add(ignoreItem);

            // Standard editor options
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new UiMenuItem { Header = "Cut", Command = ApplicationCommands.Cut });
            contextMenu.Items.Add(new UiMenuItem { Header = "Copy", Command = ApplicationCommands.Copy });
            contextMenu.Items.Add(new UiMenuItem { Header = "Paste", Command = ApplicationCommands.Paste });
        }

        private (int Offset, int Length, string Word)? GetWordAtOffset(int offset)
        {
            if (offset < 0 || offset >= _textEditor.Document.TextLength)
                return null;

            string text = _textEditor.Document.Text;
            int start = offset;
            while (start > 0 && char.IsLetter(text[start - 1]))
                start--;

            int end = offset;
            while (end < text.Length && char.IsLetter(text[end]))
                end++;

            if (start >= end)
                return null;

            return (start, end - start, text.Substring(start, end - start));
        }

        public void Dispose()
        {
            _textEditor.TextArea.ContextMenuOpening -= OnContextMenuOpening;

        }
    }

    /// <summary>
    /// Colorizer that highlights misspelled words in AvalonEdit
    /// </summary>
    public class SpellCheckColorizer : DocumentColorizingTransformer
    {
        private readonly TextBox _hiddenTextBox;

        public SpellCheckColorizer()
        {
            // Create a hidden TextBox to leverage WPF's built-in spell checking
            _hiddenTextBox = new TextBox
            {
                SpellCheck = { IsEnabled = true },
                Visibility = Visibility.Hidden
            };
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            if (line.Length == 0)
                return;

            int lineStartOffset = line.Offset;
            string text = CurrentContext.Document.GetText(line);

            // Update the hidden TextBox with current line text
            _hiddenTextBox.Text = text;

            int start = 0;
            int end = 0;

            while (start < text.Length)
            {
                // Find the next spelling error
                SpellingError error = _hiddenTextBox.GetSpellingError(start);

                if (error == null)
                    break;

                // Get the start and length of the error
                int errorStart = _hiddenTextBox.GetSpellingErrorStart(start);
                int errorLength = _hiddenTextBox.GetSpellingErrorLength(errorStart);

                if (errorStart < 0 || errorLength <= 0)
                    break;

                end = errorStart + errorLength;

                // Apply red wavy underline to the misspelled word
                ChangeLinePart(
                    lineStartOffset + errorStart,
                    lineStartOffset + end,
                    element =>
                    {
                        element.TextRunProperties.SetTextDecorations(
                            new TextDecorationCollection
                            {
                                new TextDecoration
                                {
                                    Location = TextDecorationLocation.Underline,
                                    Pen = new Pen(Brushes.Red, 1.5)
                                    {
                                        DashStyle = DashStyles.Dot
                                    }
                                }
                            });
                    });

                start = end;
            }
        }
    }
}
