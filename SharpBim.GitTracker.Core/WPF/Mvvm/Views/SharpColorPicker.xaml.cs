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

namespace SharpBIM.GitTracker.Core.WPF.Mvvm.Views
{
    /// <summary>
    /// Interaction logic for SharpColorPicker.xaml
    /// </summary>
    public partial class SharpColorPicker : UserControl
    {
        public SharpColorPicker()
        {
            InitializeComponent();
            ColorSpectrum.Fill = GenerateColorSpectrum(300, 150);
        }

        public ImageBrush GenerateColorSpectrum(int width, int height)
        {
            WriteableBitmap bmp = new WriteableBitmap(width, height, 96, 96, PixelFormats.Pbgra32, null);
            int stride = width * 4;
            byte[] pixelData = new byte[height * stride];

            for (int y = 0; y < height; y++)
            {
                double saturation = 1.0 - (y / (double)(height - 1)); // Top = Full Saturation, Bottom = Faded
                for (int x = 0; x < width; x++)
                {
                    double hue = x / (double)(width - 1) * 360.0; // Left = Red (0°), Right = Red (360°)
                    Color color = HSBtoRGB(hue, saturation, 1.0);

                    int pixelIndex = (y * stride) + (x * 4);
                    pixelData[pixelIndex] = color.B; // Blue
                    pixelData[pixelIndex + 1] = color.G; // Green
                    pixelData[pixelIndex + 2] = color.R; // Red
                    pixelData[pixelIndex + 3] = 255; // Alpha
                }
            }

            bmp.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);
            return new ImageBrush(bmp);
        }

        public Color HSBtoRGB(double h, double s, double b)
        {
            double c = b * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = b - c;
            double r = 0, g = 0, bValue = 0;

            if (h >= 0 && h < 60)
            { r = c; g = x; bValue = 0; }
            else if (h >= 60 && h < 120)
            { r = x; g = c; bValue = 0; }
            else if (h >= 120 && h < 180)
            { r = 0; g = c; bValue = x; }
            else if (h >= 180 && h < 240)
            { r = 0; g = x; bValue = c; }
            else if (h >= 240 && h < 300)
            { r = x; g = 0; bValue = c; }
            else if (h >= 300 && h < 360)
            { r = c; g = 0; bValue = x; }

            return Color.FromRgb(
                (byte)((r + m) * 255),
                (byte)((g + m) * 255),
                (byte)((bValue + m) * 255)
            );
        }
    }
}