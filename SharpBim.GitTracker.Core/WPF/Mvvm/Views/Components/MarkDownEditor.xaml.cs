using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views.Components
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
}