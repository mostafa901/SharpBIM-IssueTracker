using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpBIM.GitTracker.Core.GitHttp.Models;
using SharpBIM.GitTracker.Core.WPF.Mvvm.ViewModels;
using SharpBIM.UIContexts;
using SharpBIM.Utility;
using SharpBIM.Utility.Extensions;
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
            AddRemoveCommand = new SharpBIMCommand(async (x) => await AddRemove(x), "Add Remove Label", Glyphs.empty, (x) => true);
        }

        public string Description
        {
            get { return GetValue<string>(nameof(Description)); }
            set { SetValue(value, nameof(Description)); }
        }

        public bool IsTemplate { get; set; }

        public override void Init(GitLabel dataModel)
        {
            base.Init(dataModel);
            Title = dataModel.name;
            Description = Hint = dataModel.description;
            if (ContextData.color == null)
            {
                ContextData.color = Randoms.NextMediaColor().ToHexMedia().Substring(3);
            }
            BackgroundBrush = ("#" + ContextData.color).FromHexMedia().ToSolidColorBrush();
            BackgroundBrush.Opacity = .5;
            FontBrush = Brushes.WhiteSmoke;
        }

        public Color Brightness(Color color, Color fore)
        {
            var foreMax = Math.Max(fore.R, Math.Max(fore.G, fore.B));
            var tgtHsv = new HSV(color);

            int newValue = tgtHsv.Value + foreMax;
            int newSaturation = tgtHsv.Saturation;
            if (newValue > 255)
            {
                var newSaturation2 = newSaturation - (newValue - 255);
                newValue = 255;

                var sChRtLm = (color.R >= color.G && color.R >= color.B) ? 0.95f * 0.7f :
                              (color.G >= color.R && color.G >= color.B) ? 0.95f :
                                                                           0.95f * 0.5f;

                var sChRt = Math.Max(sChRtLm, newSaturation2 / (float)newSaturation);
                if (Single.IsInfinity(sChRt))
                    sChRt = sChRtLm;

                newSaturation = (int)(newSaturation * sChRt);
            }

            tgtHsv.Value = (byte)newValue;
            tgtHsv.Saturation = (byte)newSaturation;

            var newColor = tgtHsv.ToColor();
            return newColor;
        }

        private struct HSV
        {
            public int Hue;
            public byte Saturation;
            public byte Value;

            public HSV(Color color)
            {
                int max = Math.Max(color.R, Math.Max(color.G, color.B));
                int min = Math.Min(color.R, Math.Min(color.G, color.B));
                int div = max - min;

                if (div == 0)
                {
                    Hue = 0;
                    Saturation = 0;
                }
                else
                {
                    Hue =
                            (min == color.B) ? 60 * (color.G - color.R) / div + 60 :
                            (min == color.R) ? 60 * (color.B - color.G) / div + 180 :
                                               60 * (color.R - color.B) / div + 300;
                    Saturation = (byte)div;
                }

                Value = (byte)max;
            }

            public Color ToColor()
            {
                if (Hue == 0 && Saturation == 0)
                {
                    return Color.FromRgb(Value, Value, Value);
                }

                //byte c = Saturation;

                int HueInt = Hue / 60;

                int x = (int)(Saturation * (1 - Math.Abs((Hue / 60f) % 2 - 1)));

                Color FromRgb(int r, int g, int b)
                    => Color.FromRgb((byte)r, (byte)g, (byte)b);

                switch (Hue / 60)
                {
                    default:
                    case 0:
                        return FromRgb(Value, Value - Saturation + x, Value - Saturation);

                    case 1:
                        return FromRgb(Value - Saturation + x, Value, Value - Saturation);

                    case 2:
                        return FromRgb(Value - Saturation, Value, Value - Saturation + x);

                    case 3:
                        return FromRgb(Value - Saturation, Value - Saturation + x, Value);

                    case 4:
                        return FromRgb(Value - Saturation + x, Value - Saturation, Value);

                    case 5:
                    case 6:
                        return FromRgb(Value, Value - Saturation, Value - Saturation + x);
                }
            }
        }

        public SharpBIMCommand AddRemoveCommand { get; set; }

        // Add this line to the constructor

        public bool IsAvailable { get; set; }

        public bool IsSelected
        {
            get { return GetValue<bool>(nameof(IsSelected)); }
            set { SetValue(value, nameof(IsSelected)); }
        }

        public async Task AddRemove(object x)
        {
            try
            {
                var issuemv = GetParentViewModel<IssueViewModel>();
                if (x == null)
                {
                    issuemv.IssueLables.Remove(this);
                }
                else
                {
                    if (((LabelModelView)x).Title != Title)
                        return;
                    await issuemv.AddLabelTextChanged(Title + "\t");
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}