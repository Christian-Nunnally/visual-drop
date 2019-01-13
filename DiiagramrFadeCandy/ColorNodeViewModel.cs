using DiiagramrAPI.PluginNodeApi;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiiagramrFadeCandy
{
    public class ColorNodeViewModel : PluginNode
    {
        public Terminal<bool> PickRandomTriggerTerminal { get; private set; }
        public Terminal<Color> ColorOutputTerminal { get; private set; }
        public Bitmap ColorWheelBitmap { get; set; }
        public BitmapImage ColorWheelBitmapImage { get; set; }
        public SolidColorBrush SelectedColorBrush { get; set; }

        [PluginNodeSetting]
        public Color C { get; set; } = new Color(0, 0, 0, 0);

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeName("Color Picker");
            setup.NodeSize(60, 60);

            PickRandomTriggerTerminal = setup.InputTerminal<bool>("Pick Random", Direction.North);
            PickRandomTriggerTerminal.DataChanged += PickRandomTriggerTerminal_DataChanged;

            ColorOutputTerminal = setup.OutputTerminal<Color>("Color", Direction.South);

            ColorWheelBitmap = Properties.Resources.lightcolorspectrum;
            ColorWheelBitmapImage = BitmapToImageSource(ColorWheelBitmap);
        }

        private void PickRandomTriggerTerminal_DataChanged(bool data)
        {
            if (data)
            {
                var random = new Random();
                var randomBytes = new byte[3];
                random.NextBytes(randomBytes);
                var floatR = 0f;
                var floatG = 0f;
                var floatB = 0f;
                if (random.Next(2) > 0)
                {
                    if (random.Next(2) > 0)
                    {
                        floatR = 1.0f / 255.0f * randomBytes[0];
                        floatG = 0.3f / 255.0f * randomBytes[1];
                        floatB = 0.3f / 255.0f * randomBytes[2];
                    }
                    else
                    {
                        floatR = 0.3f / 255.0f * randomBytes[0];
                        floatG = 0.3f / 255.0f * randomBytes[1];
                        floatB = 1.0f / 255.0f * randomBytes[2];
                    }
                }
                else
                {
                    if (random.Next(2) > 0)
                    {
                        floatR = 0.3f / 255.0f * randomBytes[0];
                        floatG = 1.0f / 255.0f * randomBytes[1];
                        floatB = 0.3f / 255.0f * randomBytes[2];
                    }
                    else
                    {
                        floatR = 0.3f / 255.0f * randomBytes[0];
                        floatG = 0.3f / 255.0f * randomBytes[1];
                        floatB = 0.3f / 255.0f * randomBytes[2];
                    }
                }
                ColorOutputTerminal.Data = new Color(floatR, floatG, floatB, 255);
                SelectedColorBrush = new SolidColorBrush(new System.Windows.Media.Color() { R = randomBytes[0], G = randomBytes[1], B = randomBytes[2] });
            }
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public void ColorWheelMouseDown(object sender, MouseButtonEventArgs e)
        {
            var frameworkElement = (FrameworkElement)sender;
            System.Windows.Point position;
            if (frameworkElement != null && frameworkElement.IsMouseOver)
            {
                position = e.GetPosition(frameworkElement);
            }
            else
            {
                position = new System.Windows.Point(-1, -1);
            }

            if (Width == double.NaN || Width == 0)
            {
                return;
            }

            if (Height == double.NaN || Height == 0)
            {
                return;
            }

            var xRelativeToBitmap = ColorWheelBitmap.Width / Width * position.X;
            var yRelativeToBitmap = ColorWheelBitmap.Height / Height * position.Y;

            var color = ColorWheelBitmap.GetPixel((int)xRelativeToBitmap, (int)yRelativeToBitmap);

            var floatR = 1.0f / 255.0f * color.R;
            var floatG = 1.0f / 255.0f * color.G;
            var floatB = 1.0f / 255.0f * color.B;
            var floatA = 1.0f / 255.0f * color.A;
            ColorOutputTerminal.Data = new Color(floatR, floatG, floatB, floatA);
            SelectedColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
