using DiiagramrAPI.PluginNodeApi;
using SharpDX.Mathematics.Interop;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DiiagramrFadeCandy
{
    [KnownType(typeof(RawColor4))]
    public class ColorNodeViewModel : PluginNode
    {
        public Terminal<Color> ColorOutputTerminal { get; private set; }
        public Bitmap ColorWheelBitmap { get; set; }
        public BitmapImage ColorWheelBitmapImage { get; set; }
        public SolidColorBrush SelectedColorBrush { get; set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeName("Color Picker");
            setup.NodeSize(60, 60);

            ColorOutputTerminal = setup.OutputTerminal<Color>("Color", Direction.East);

            ColorWheelBitmap = Properties.Resources.colourwheel;
            ColorWheelBitmapImage = BitmapToImageSource(ColorWheelBitmap);
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