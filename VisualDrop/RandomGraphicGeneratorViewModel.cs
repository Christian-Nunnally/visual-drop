using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DiiagramrAPI.PluginNodeApi;
using Stylet;

namespace VisualDrop
{
    public class RandomGraphicGeneratorViewModel : PluginNode
    {
        private int _numberOfLights;
        private readonly VisualEffect _visualEffect = new VisualEffect(2);


        private readonly Dictionary<string, Func<int, double, byte[]>> _graphicGenerators = new Dictionary<string, Func<int, double, byte[]>>();
        private readonly Dictionary<string, Func<int, Color>> _colorFunctions = new Dictionary<string, Func<int, Color>>();

        public ObservableCollection<string> GeneratorOptions { get; set; } = new BindableCollection<string>();
        public string SelectedGenerator { get; set; }

        private Random rnd = new Random();

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(160, 120);
            setup.NodeName("RandomGraphicGenerator");
            GraphicOutput = setup.OutputTerminal<byte[]>("Generated Graphic", Direction.West);
            GenerateGraphicTerminal = setup.InputTerminal<bool>("Generate Graphic", Direction.North);

            setup.InputTerminal<bool>("On/Off", Direction.North).DataChanged += OnDataChanged;
            var outputTerminal = setup.OutputTerminal<IVisualEffect>("Effect", Direction.South);

            AddGraphicMode("HV Mirror", HorizontialVerticalMirror);
            AddColorMode("Random", GetTotallyRandomColor);
            AddColorMode("Color", GetRandomPureColor);

            SelectedColor = ColorOptions.FirstOrDefault();
            SelectedGenerator = GeneratorOptions.FirstOrDefault();
            NumberOfLightsText = "64";
            Brightness = 128;
            DensityPercent = 50;
            _numberOfLights = 64;

            GenerateGraphicTerminal.DataChanged += GenerateGraphicTerminalOnDataChanged;

            _visualEffect.SetFrame(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0);
            _visualEffect.SetFrame(new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 }, 1);
            outputTerminal.Data = _visualEffect;
        }

        private void OnDataChanged(bool data) 
        {
            _visualEffect.CurrentFrame = data ? 1 : 0;
        }

        private void AddGraphicMode(string id, Func<int, double, byte[]> modeFunction)
        {
            _graphicGenerators.Add(id, modeFunction);
            GeneratorOptions.Add(id);
        }

        private void AddColorMode(string id, Func<int, Color> modeFunction)
        {
            _colorFunctions.Add(id, modeFunction);
            ColorOptions.Add(id);
        }

        private void GenerateGraphicTerminalOnDataChanged(bool data)
        {
            if (data)
            {
                GenerateGraphic();
            }
        }

        public Terminal<bool> GenerateGraphicTerminal { get; set; }

        private byte[] HorizontialVerticalMirror(int numberOfLeds, double density)
        {
            var size = Math.Sqrt(numberOfLeds);
            var intSize = (int) size;
            if (size % 1 != 0 || intSize % 2 != 0) return null;
            var topCornerWidth = intSize / 2;
            var graphic = new StaticLedGraphic(numberOfLeds);

            var selectedLights = new bool[topCornerWidth * topCornerWidth];
            var numOfOnLights = (int)(density / 100.0 * selectedLights.Length);
            numOfOnLights = Math.Max(1, numOfOnLights);

            var pixel = rnd.Next(selectedLights.Length);
            for (var i = 0; i < numOfOnLights; i++)
            {
                selectedLights[pixel] = true;
                pixel += rnd.Next(-1, 2) * intSize;
                pixel += rnd.Next(-1, 2) * 1;
                if (pixel < 0) pixel = selectedLights.Length + pixel;
                pixel = pixel % selectedLights.Length;
            }

            var color = _colorFunctions[SelectedColor].Invoke(Brightness);
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

        public ObservableCollection<string> ColorOptions { get; set; } = new ObservableCollection<string>();

        public string SelectedColor { get; set; }

        public int Brightness { get; set; }

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

            if (GraphicOutput.Data == null) return;
            if (_visualEffect.Frames[0].Length != GraphicOutput.Data.Length) _visualEffect.SetFrame(new byte[GraphicOutput.Data.Length], 0);
            _visualEffect.SetFrame(GraphicOutput.Data, 1);
        }

        private Color GetTotallyRandomColor(int brightness)
        {
            return Color.FromArgb(255, (byte) rnd.Next(brightness), (byte) rnd.Next(brightness), (byte) rnd.Next(brightness));
        }

        private Color GetRandomPureColor(int brightness)
        {
            var colorType = rnd.Next(7);

            switch (colorType)
            {
                case 0:
                    return Color.FromArgb(255, (byte)brightness, 0, 0);
                case 1:
                    return Color.FromArgb(255, 0, (byte)brightness, 0);
                case 2:
                    return Color.FromArgb(255, 0, 0, (byte)brightness);
                case 3:
                    return Color.FromArgb(255, (byte)brightness, (byte)brightness, 0);
                case 4:
                    return Color.FromArgb(255, 0, (byte)brightness, (byte)brightness);
                case 5:
                    return Color.FromArgb(255, (byte)brightness, 0, (byte)brightness);
                case 6:
                    return Color.FromArgb(255, (byte)brightness, (byte)brightness, (byte)brightness);
            }
            return Colors.Black;
        }
    }
}
