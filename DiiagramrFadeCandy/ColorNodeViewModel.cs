using DiiagramrAPI.Diagram;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiiagramrFadeCandy
{
    public class ColorNodeViewModel : Node
    {
        public TypedTerminal<bool> PickRandomTriggerTerminal { get; private set; }
        public TypedTerminal<Color> ColorOutputTerminal { get; private set; }
        public Bitmap ColorWheelBitmap { get; set; }
        public BitmapImage ColorWheelBitmapImage { get; set; }
        public TypedTerminal<float> RedInputTerminal { get; private set; }
        public TypedTerminal<float> BlueInputTerminal { get; private set; }
        public TypedTerminal<float> GreenInputTerminal { get; private set; }
        public TypedTerminal<float> AlphaInputTerminal { get; private set; }
        public SolidColorBrush SelectedColorBrush { get; set; }
        public string ClickPoint { get; set; }

        public bool IsColorPickerVisible { get; set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeName("Color Picker");
            setup.NodeSize(60, 60);

            PickRandomTriggerTerminal = setup.InputTerminal<bool>("Pick Random", Direction.East);
            PickRandomTriggerTerminal.DataChanged += PickRandomTriggerTerminal_DataChanged;

            ColorOutputTerminal = setup.OutputTerminal<Color>("Color", Direction.South);

            ColorWheelBitmap = Properties.Resources.lightcolorspectrum;
            ColorWheelBitmapImage = BitmapToImageSource(ColorWheelBitmap);

            RedInputTerminal = setup.InputTerminal<float>("Red", Direction.North);
            BlueInputTerminal = setup.InputTerminal<float>("Blue", Direction.North);
            GreenInputTerminal = setup.InputTerminal<float>("Green", Direction.North);
            AlphaInputTerminal = setup.InputTerminal<float>("Alpha", Direction.West);

            RedInputTerminal.DataChanged += RedInputTerminal_DataChanged;
            RedInputTerminal.DataChanged += GreenInputTerminal_DataChanged;
            RedInputTerminal.DataChanged += BlueInputTerminal_DataChanged;
            RedInputTerminal.DataChanged += AlphaInputTerminal_DataChanged;
        }

        private void RedInputTerminal_DataChanged(float data)
        {
            if (ColorOutputTerminal.Data != null)
            {
                SetColorOnTerminal(data, ColorOutputTerminal.Data.G, ColorOutputTerminal.Data.B, ColorOutputTerminal.Data.A);
            }
        }

        private void GreenInputTerminal_DataChanged(float data)
        {
            if (ColorOutputTerminal.Data != null)
            {
                SetColorOnTerminal(ColorOutputTerminal.Data.R, data, ColorOutputTerminal.Data.B, ColorOutputTerminal.Data.A);
            }
        }

        private void BlueInputTerminal_DataChanged(float data)
        {
            if (ColorOutputTerminal.Data != null)
            {
                SetColorOnTerminal(ColorOutputTerminal.Data.R, ColorOutputTerminal.Data.G, data, ColorOutputTerminal.Data.A);
            }
        }

        private void AlphaInputTerminal_DataChanged(float data)
        {
            if (ColorOutputTerminal.Data != null)
            {
                SetColorOnTerminal(ColorOutputTerminal.Data.R, ColorOutputTerminal.Data.G, ColorOutputTerminal.Data.B, data);
            }
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
                SetColorOnTerminal(floatR, floatG, floatB, 1.0f);
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
                position = new System.Windows.Point(-3, -3);
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

            ClickPoint = $"{position.X.ToString("0,0")}, {position.Y.ToString("0,0")}";

            var color = ColorWheelBitmap.GetPixel((int)xRelativeToBitmap, (int)yRelativeToBitmap);

            var floatR = 1.0f / 255.0f * color.R;
            var floatG = 1.0f / 255.0f * color.G;
            var floatB = 1.0f / 255.0f * color.B;
            var floatA = 1.0f / 255.0f * color.A;
            SetColorOnTerminal(floatR, floatG, floatB, floatA);
        }

        private void SetColorOnTerminal(float floatR, float floatG, float floatB, float floatA)
        {
            ColorOutputTerminal.Data = new Color(floatR, floatG, floatB, floatA);
            SelectedColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(floatR * 255.0), (byte)(floatG * 255.0), (byte)(floatB * 255.0), (byte)(floatA * 255.0)));
        }

        protected override void MouseEnteredNode()
        {
            IsColorPickerVisible = true;
        }

        protected override void MouseLeftNode()
        {
            IsColorPickerVisible = false;
        }
    }
}
