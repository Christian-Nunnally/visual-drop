using DiiagramrAPI.PluginNodeApi;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

namespace DiiagramrFadeCandy
{
    [HideFromNodeSelector]
    public class TextEffectNodeViewModel : PluginNode, IGraphicEffect
    {
        private bool _isCachedRectangleValid;
        private readonly float _x;
        private readonly float _y;
        private readonly float _width;
        private readonly float _height;
        private RawRectangleF _cachedRectangle = new RawRectangleF(0, 0, 8, 8);
        private Color _color = new Color(0.5f, 0.0f, 0.2f, 1.0f);


        public Terminal<bool> VisibleTerminal { get; private set; }
        public Terminal<float> XTerminal { get; private set; }
        public Terminal<float> YTerminal { get; private set; }
        public Terminal<float> WidthTerminal { get; private set; }
        public Terminal<float> HeightTerminal { get; private set; }
        public Terminal<IGraphicEffect> EffectTerminal { get; private set; }
        public Terminal<Color> ColorTerminal { get; private set; }

        public string DisplayText { get; set; }

        public TextFormat TextFormat { get; set; } = new SharpDX.DirectWrite.TextFormat(new SharpDX.DirectWrite.Factory(), "Consolas", 10);

        public RawRectangleF Rectangle
        {
            get
            {
                if (_isCachedRectangleValid)
                {
                    return _cachedRectangle;
                }

                _cachedRectangle = new RawRectangleF(XTerminal.Data, YTerminal.Data, WidthTerminal.Data, HeightTerminal.Data);
                _isCachedRectangleValid = true;
                return _cachedRectangle;
            }

            set
            {
                _isCachedRectangleValid = true;
                _cachedRectangle = value;
            }
        }

        public float FontSize { get; private set; }

        public void Draw(RenderTarget target)
        {
            if (!VisibleTerminal.Data)
            {
                return;
            }

            var color = new RawColor4(_color.R, _color.G, _color.B, _color.A);
            var brush = new SolidColorBrush(target, color);
            target.DrawText(DisplayText, TextFormat, Rectangle, brush);
        }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("Text Effect");

            VisibleTerminal = setup.InputTerminal<bool>("Visible", Direction.North);
            VisibleTerminal.Data = true;

            XTerminal = setup.InputTerminal<float>("X", Direction.West);
            XTerminal.DataChanged += XTerminalDataChanged;
            YTerminal = setup.InputTerminal<float>("Y", Direction.West);
            YTerminal.DataChanged += YTerminalDataChanged;

            WidthTerminal = setup.InputTerminal<float>("Width", Direction.East);
            WidthTerminal.DataChanged += WidthTerminalDataChanged;
            HeightTerminal = setup.InputTerminal<float>("Height", Direction.East);
            HeightTerminal.DataChanged += WidthTerminalDataChanged;

            ColorTerminal = setup.InputTerminal<Color>("Color", Direction.North);
            ColorTerminal.DataChanged += ColorTerminalTerminalDataChanged;
            EffectTerminal = setup.OutputTerminal<IGraphicEffect>("Effect", Direction.South);
            EffectTerminal.Data = this;
        }

        private void ColorTerminalTerminalDataChanged(Color color)
        {
            if (color == null)
            {
                return;
            }

            _color = color;
        }

        private void XTerminalDataChanged(float x)
        {
            _isCachedRectangleValid = false;
        }

        private void YTerminalDataChanged(float y)
        {
            _isCachedRectangleValid = false;
        }

        private void WidthTerminalDataChanged(float width)
        {
            _isCachedRectangleValid = false;
        }

        private void HeightTerminalDataChanged(float height)
        {
            _isCachedRectangleValid = false;
        }
    }
}