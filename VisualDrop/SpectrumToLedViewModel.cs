using System;
using System.Linq;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class SpectrumToLedViewModel : PluginNode
    {
        private int blueIndex;
        private int bot;
        private int frame;
        private int greenIndex;
        private int redIndex;
        private readonly byte[] rgb = new byte[3];

        private Color solidColor = Colors.Black;

        private int top;
        private bool _displaysInitialized { get; set; }

        public Terminal<byte[]> SerialOutput { get; set; }

        public Terminal<byte[]> SpectrumInput { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("SpectrumToLed");

            SpectrumInput = setup.InputTerminal<byte[]>("Spectrum In", Direction.North);
            SerialOutput = setup.OutputTerminal<byte[]>("Serial Data", Direction.South);

            SpectrumInput.DataChanged += SpectrumInputOnDataChanged;
        }

        private void SpectrumInputOnDataChanged(byte[] data)
        {
            if (data == null) return;
            if (data.Length < 8 || data.Length % 2 != 0) return;

            var header1 = new byte[] {40, 30, 20, 1, 0 };
            var header2 = new byte[] {40, 30, 20, 1, 1 };
            var header3 = new byte[] {40, 30, 20, 1, 2 };

            var led1 = new StaticLedGraphic(128);
            var led2 = new StaticLedGraphic(128);

            var perDisplayBars = data.Length / 2;
            var ledsPerBar = 16 / perDisplayBars;

            for (var i = 0; i < perDisplayBars; i++)
            for (var w = 0; w < ledsPerBar; w++)
            {
                var col = i * ledsPerBar + w; 
                var lowBarValue = data[i];
                var highBarValue = data[i + perDisplayBars];

                for (var row = 0; row < 8; row++)
                {
                    var pixelNumber = col * 8 + row;

                    var tempPixelNumber = pixelNumber >= 64 ? pixelNumber - 64 : pixelNumber;
                    var displayRow = 7 - tempPixelNumber % 8;
                    var displayCol = (int) Math.Floor((decimal) (tempPixelNumber / 8));
                    var newPixelNumber = displayRow * 8 + displayCol;
                    pixelNumber = pixelNumber >= 64 ? newPixelNumber + 64 : newPixelNumber;

                    var ledValue = (byte) (255 / 7 * row);
                    var color = lowBarValue > ledValue ? GetColorFromValue(row) : Colors.Black;
                    led2.SetPixel(pixelNumber >= 64 ? pixelNumber - 64 : pixelNumber + 64, color);

                    color = highBarValue > ledValue ? GetColorFromValue(row) : Colors.Black;
                    led1.SetPixel(pixelNumber, color);
                }
            }

            frame++;
            var rnd = new Random();
            if (frame % 300 == 0)
            {
                redIndex = rnd.Next(0, 3);
                while (blueIndex == redIndex) blueIndex = rnd.Next(0, 3);
                while (greenIndex == redIndex || greenIndex == blueIndex) greenIndex = rnd.Next(0, 3);
                solidColor = Color.FromArgb(255, (byte) (rnd.Next(3) == 0 ? 255 : 0), (byte) (rnd.Next(3) == 0 ? 255 : 0), (byte) (rnd.Next(3) == 0 ? 255 : 0));
            }
            var led3 = new StaticLedGraphic(150);

            var averageLows = (data[0] + data[1] + data[2]) / 3;
            var averageHighs = (data[16] + data[17]) / 2;
            top = Math.Max(top, averageHighs);
            bot = Math.Min(bot, averageHighs);

            top -= 3;
            bot += 3;

            var on = averageHighs - bot > 0.5 * top;


            var value = 150 / 255.0 * averageLows;
            for (var i = 0; i < 50; i++)
            {
                var r = (byte) (value > i + 0 ? 255 : 0);
                var g = (byte) (value > i + 75 ? 255 : 0);
                rgb[0] = r;
                rgb[1] = g;
                rgb[2] = 0;
                var color = Color.FromArgb(255, rgb[redIndex], rgb[greenIndex], rgb[blueIndex]);
                led3.SetPixel(i, color);
                led3.SetPixel(150 - i, color);
            }

            var sColor = on ? solidColor : Colors.Black;
            for (var i = 50; i <= 100; i++)
                led3.SetPixel(i, sColor);

            if (!_displaysInitialized)
            {
                _displaysInitialized = true;
                SerialOutput.Data = new byte[] {40, 30, 20, 0, 0, 128};
                SerialOutput.Data = new byte[] {40, 30, 20, 0, 1, 128};
                SerialOutput.Data = new byte[] {40, 30, 20, 0, 2, 150};
            }

            SerialOutput.Data = null;
            SerialOutput.Data = header2.Concat(led2.Graphic).ToArray();
            SerialOutput.Data = header1.Concat(led1.Graphic).ToArray();
            SerialOutput.Data = header3.Concat(led3.Graphic).ToArray();
        }

        private Color GetColorFromValue(int height)
        {
            return Color.FromArgb(0, (byte) (15 * height), (byte) (49 - 7 * height), (byte) (height == 7 ? 150 : 0));
        }
    }
}