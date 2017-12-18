using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class SpectrumToLedViewModel : PluginNode
    {
        private bool _displaysInitialized { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("SpectrumToLed");

            SpectrumInput = setup.InputTerminal<List<byte>>("Spectrum In", Direction.North);
            SerialOutput = setup.OutputTerminal<byte[]>("Serial Data", Direction.South);

            SpectrumInput.DataChanged += SpectrumInputOnDataChanged;
        }

        private void SpectrumInputOnDataChanged(List<byte> data)
        {
            if (data == null) return;
            if (data.Count < 8 || data.Count % 2 != 0) return; 

            var header1 = new byte[] { 40, 30, 20, 10, 1, 0, 0, 0, 0, 0, 0, 0 };
            var header2 = new byte[] { 40, 30, 20, 10, 1, 1, 0, 0, 0, 0, 0, 0 };

            var led1 = new StaticLedGraphic(128);
            var led2 = new StaticLedGraphic(128);

            var perDisplayBars = data.Count / 2;
            var ledsPerBar = 16 / perDisplayBars;

            for (int i = 0; i < perDisplayBars; i++)
            {
                for (int w = 0; w < ledsPerBar; w++) 
                {
                    var col = i * ledsPerBar + w;
                    var lowBarValue = data[i];
                    var highBarValue = data[i + perDisplayBars]; 

                    for (int row = 0; row < 8; row++)
                    {
                        var pixelNumber = col * 8 + row;

                        var tempPixelNumber = pixelNumber >= 64 ? pixelNumber - 64 : pixelNumber;
                        var displayRow = 7 - tempPixelNumber % 8;
                        var displayCol = (int)Math.Floor((decimal) (tempPixelNumber / 8));
                        var newPixelNumber = displayRow * 8 + displayCol;
                        pixelNumber = pixelNumber >= 64 ? newPixelNumber + 64 : newPixelNumber;

                        var ledValue = (byte) (255 / 7 * row);
                        var color = lowBarValue > ledValue ? GetColorFromValue(row) : Colors.Black;
                        led2.SetPixel(pixelNumber >= 64 ? pixelNumber - 64 : pixelNumber + 64, color);

                        color = highBarValue > ledValue ? GetColorFromValue(row) : Colors.Black;
                        led1.SetPixel(pixelNumber, color);
                    }
                }
            }

            if (!_displaysInitialized)
            {
                _displaysInitialized = true;
                SerialOutput.Data = new byte[] { 40, 30, 20, 10, 0, 0, 0, 0, 0, 0, 0, 0, 128 };
                SerialOutput.Data = new byte[] { 40, 30, 20, 10, 0, 1, 0, 0, 0, 0, 0, 0, 128 };
            }

            SerialOutput.Data = null;
            SerialOutput.Data = header2.Concat(led2.Graphic).ToArray();
            SerialOutput.Data = header1.Concat(led1.Graphic).ToArray();
        }

        private Color GetColorFromValue(int height)
        {
            //return Color.FromArgb(0, 255, 255, 255);
            return Color.FromArgb(0, (byte) (15 * height), (byte) (49 - (7 * height)), (byte) (height == 7 ? 150 : 0));
        }

        public Terminal<byte[]> SerialOutput { get; set; }

        public Terminal<List<byte>> SpectrumInput { get; set; }
    }
}
