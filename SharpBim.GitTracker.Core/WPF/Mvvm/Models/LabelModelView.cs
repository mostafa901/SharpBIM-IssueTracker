using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.UIContexts;
using SharpBIM.Utility;
using SharpBIM.WPF.Assets;
using SharpBIM.WPF.Assets.Fonts;
using SharpBIM.WPF.Helpers.Commons;

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Models
{
    public class LabelModelView : ModelViewBase<GitLabel>
    {
        public SolidColorBrush BackgroundBrush
        {
            get { return GetValue<SolidColorBrush>(nameof(BackgroundBrush)); }
            set { SetValue(value, nameof(BackgroundBrush)); }
        }

        public SolidColorBrush FontBrush
        {
            get { return GetValue<SolidColorBrush>(nameof(FontBrush)); }
            set { SetValue(value, nameof(FontBrush)); }
        }

        public LabelModelView()
        {
            AddRemoveCommand = new SharpBIMCommand(AddRemove, "Add Remove Label", Glyphs.empty, (x) => true);
        }

        public override void Init(GitLabel dataModel)
        {
            base.Init(dataModel);
            Title = dataModel.name;
            Hint = dataModel.description;
            AddRemove(null);
        }

        public SharpBIMCommand AddRemoveCommand { get; set; }

        // Add this line to the constructor

        public bool IsAvailable { get; set; }

        public void AddRemove(object x)
        {
            try
            {
                IsAvailable = !IsAvailable;
                if (IsAvailable)
                { 
                    BackgroundBrush = ("#" + ContextData.color).FromHexMedia().ToSolidColorBrush();
                  
                        FontBrush = new SolidColorBrush(ResourceValues.SolidColorBrushs.ButtonFontBrush.Color) {   };
                    }
                else
                {
                    FontBrush = new SolidColorBrush(ResourceValues.SolidColorBrushs.ButtonFontBrush.Color) { Opacity = .2 };
                    BackgroundBrush = new SolidColorBrush(FontBrush.Color)
                    {
                        Opacity = .2
                    };
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}