using DiiagramrAPI.PluginNodeApi;
using System;

namespace DiiagramrFadeCandy
{
    public class ShapeEffectNodeViewModel : PluginNode
    {
        public Terminal<bool> VisibleTerminal { get; private set; }
        public Terminal<bool> FillTerminal { get; private set; }
        public Terminal<float> XTerminal { get; private set; }
        public Terminal<float> YTerminal { get; private set; }
        public Terminal<float> WidthTerminal { get; private set; }
        public Terminal<float> HeightTerminal { get; private set; }
        public Terminal<IGraphicEffect> EffectTerminal { get; private set; }
        public Terminal<Color> ColorTerminal { get; private set; }
        public Terminal<float> BrightnessTerminal { get; private set; }

        public ShapeEffect ShapeGraphic { get; set; } = new ShapeEffect();

        public Shape Mode { get; set; }
        public string SelectedShapeString
        {
            get => ShapeGraphic.Mode.ToString();
            set
            {
                if (Enum.TryParse(value, out Shape _mode))
                {
                    ShapeGraphic.Mode = _mode;
                }
            }
        }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("Shape Effect");

            VisibleTerminal = setup.InputTerminal<bool>("Visible", Direction.North);
            VisibleTerminal.DataChanged += v => ShapeGraphic.Visible = v;
            VisibleTerminal.Data = true;
            VisibleTerminal = setup.InputTerminal<bool>("Fill", Direction.North);
            VisibleTerminal.DataChanged += f => ShapeGraphic.Fill = f;
            VisibleTerminal.Data = true;

            XTerminal = setup.InputTerminal<float>("X", Direction.West);
            XTerminal.Data = 4;
            XTerminal.DataChanged += x => ShapeGraphic.X = x;
            YTerminal = setup.InputTerminal<float>("Y", Direction.West);
            YTerminal.Data = 4;
            YTerminal.DataChanged += y => ShapeGraphic.Y = y;
            WidthTerminal = setup.InputTerminal<float>("Width", Direction.East);
            WidthTerminal.Data = 3;
            WidthTerminal.DataChanged += w => ShapeGraphic.Width = w;
            HeightTerminal = setup.InputTerminal<float>("Height", Direction.East);
            HeightTerminal.Data = 3;
            HeightTerminal.DataChanged += h => ShapeGraphic.Height = h;
            BrightnessTerminal = setup.InputTerminal<float>("Brightness", Direction.East);
            BrightnessTerminal.DataChanged += BrightnessTerminalDataChanged;
            BrightnessTerminal.Data = 1.0f;

            ColorTerminal = setup.InputTerminal<Color>("Color", Direction.North);
            ColorTerminal.DataChanged += ColorTerminalTerminalDataChanged;
            EffectTerminal = setup.OutputTerminal<IGraphicEffect>("Effect", Direction.South);
            EffectTerminal.Data = ShapeGraphic;
        }

        private void BrightnessTerminalDataChanged(float brightness)
        {
            ShapeGraphic.A = brightness;
        }

        private void ColorTerminalTerminalDataChanged(Color color)
        {
            if (color == null)
            {
                return;
            }

            ShapeGraphic.R = color.R;
            ShapeGraphic.G = color.G;
            ShapeGraphic.B = color.B;
            ShapeGraphic.A = color.A;
        }
    }
}