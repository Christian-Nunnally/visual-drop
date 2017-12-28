using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;
using Stylet;

namespace VisualDrop
{
    public class RandomGraphicGeneratorViewModel : PluginNode
    {
        private int _numberOfLights;

        private Dictionary<string, Func<int, double, byte[]>> _graphicGenerators = new Dictionary<string, Func<int, double, byte[]>>();

        public ObservableCollection<string> GeneratorOptions { get; set; } = new BindableCollection<string>();
        public string SelectedGenerator { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(160, 120);
            setup.NodeName("RandomGraphicGenerator");
            GraphicOutput = setup.OutputTerminal<byte[]>("Generated Graphic", Direction.West);

            _graphicGenerators.Add("HV Mirror", HorizontialVerticalMirror);
            GeneratorOptions.Add("HV Mirror");

            SelectedGenerator = GeneratorOptions.FirstOrDefault();
            NumberOfLightsText = "64";
            _numberOfLights = 64;
        }

        private byte[] HorizontialVerticalMirror(int numberOfLeds, double density)
        {
            var rnd = new Random();
            var size = Math.Sqrt(numberOfLeds);
            var intSize = (int) size;
            if (size % 1 != 0 || intSize % 2 != 0) return null;
            var topCornerWidth = intSize / 2;
            var graphic = new StaticLedGraphic(numberOfLeds);

            var selectedLights = new bool[topCornerWidth * topCornerWidth];
            var numOfOnLights = (int)(density / 100.0 * selectedLights.Length);

            var pixel = rnd.Next(selectedLights.Length);
            for (var i = 0; i < numOfOnLights; i++)
            {
                selectedLights[pixel] = true;
                pixel += rnd.Next(-1, 2) * intSize;
                pixel += rnd.Next(-1, 2) * 1;
                if (pixel < 0) pixel = selectedLights.Length + pixel;
                pixel = pixel % selectedLights.Length;
            }

            var color = GetRandomColor();
            for (int x = 0; x < topCornerWidth; x++)
            {
                for (int y = 0; y < topCornerWidth; y++)
                {
                    var topCornerPixel = x * topCornerWidth + y;
                    if (selectedLights[topCornerPixel])
                    {
                        var nwOnDisplay = x * intSize + y;

                        var neOnDisplay = (intSize - x - 1) * intSize + y;

                        var swOnDisplay = x * intSize + (intSize - y - 1);

                        var seOnDisplay = (intSize - x - 1) * intSize + (intSize - y - 1);

                        graphic.SetPixel(nwOnDisplay, color);
                        graphic.SetPixel(neOnDisplay, color);
                        graphic.SetPixel(swOnDisplay, color);
                        graphic.SetPixel(seOnDisplay, color);
                    }
                }
            }

            return graphic.Graphic;
        }

        public Terminal<byte[]> GraphicOutput { get; set; }

        public string NumberOfLightsText
        {
            get => _numberOfLights.ToString();
            set
            {
                if (!int.TryParse(value, out var result)) return;
                _numberOfLights = result;
            }
        }

        public int DensityPercent { get; set; }

        public void PreviewNumberOfLightsTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(text);
        }

        public void GenerateGraphic()
        {
            if (!_graphicGenerators.ContainsKey(SelectedGenerator)) return;
            GraphicOutput.Data = _graphicGenerators[SelectedGenerator].Invoke(_numberOfLights, DensityPercent);
        }

        private Color GetRandomColor()
        {
            var rnd = new Random();
            return Color.FromArgb(255, (byte) rnd.Next(256), (byte) rnd.Next(256), (byte) rnd.Next(256));
        }
    }
}
