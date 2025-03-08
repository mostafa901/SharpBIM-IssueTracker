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
using MdXaml;
using MdXaml.Plugins;
using SharpBIM.GitTracker.Core.WPF.Helpers.MdPlugins;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views.Components
{
    /// <summary>
    /// Interaction logic for MarkDownViewer.xaml
    /// </summary>
    public partial class MarkDownViewer : MarkdownScrollViewer

    {
        public MarkDownViewer()
        {
            InitializeComponent();
            Plugins = MdXamlPlugins.Default;
            Plugins.Inline.Add(new CheckboxInlineParser());
        }
    }
}