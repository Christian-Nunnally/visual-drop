using DiiagramrAPI.PluginNodeApi;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace DiiagramrFadeCandy
{
    public class ShapeEffectNodeViewModel : PluginNode, IGraphicEffect
    {
        private bool _isCachedElipseValid;
        private float _x;
        private float _y;
        private float _width;
        private float _height;
        private bool _isCachedPositionValid;
        private Ellipse _cachedEllipse = new Ellipse(new RawVector2(4, 4), 3, 3);
        private Color _color = new Color(0.5f, 0.0f, 0.2f, 1.0f);
        private RawVector2 _cachedPosition;

        public Terminal<bool> VisibleTerminal { get; private set; }
        public Terminal<int> XTerminal { get; private set; }
        public Terminal<int> YTerminal { get; private set; }

        public Terminal<float> WidthTerminal { get; private set; }
        public Terminal<float> HeightTerminal { get; private set; }
        public Terminal<IGraphicEffect> EffectTerminal { get; private set; }
        public Terminal<Color> ColorTerminal { get; private set; }

        public RawVector2 Position
        {
            get
            {
                if (_isCachedPositionValid)
                {
                    return _cachedPosition;
                }

                _cachedPosition = new RawVector2(_x, _y);
                _isCachedPositionValid = true;
                return _cachedPosition;
            }
            set
            {
                _isCachedElipseValid = false;
                _isCachedPositionValid = true;
                _cachedPosition = value;
            }
        }

        public Ellipse Ellipse
        {
            get
            {
                if (_isCachedElipseValid)
                {
                    return _cachedEllipse;
                }

                _cachedEllipse = new Ellipse(Position, 3, 3);
                _isCachedElipseValid = true;
                return _cachedEllipse;
            }

            set
            {
                _isCachedElipseValid = true;
                _cachedEllipse = value;
            }
        }

        public void Draw(RenderTarget target)
        {
            if (!VisibleTerminal.Data)
            {
                return;
            }

            target.DrawEllipse(Ellipse, new SolidColorBrush(target, new RawColor4(_color.R, _color.G, _color.B, _color.A)));
        }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("Shape Effect");

            VisibleTerminal = setup.InputTerminal<bool>("Visible", Direction.North);
            VisibleTerminal.Data = true;

            XTerminal = setup.InputTerminal<int>("X", Direction.West);
            XTerminal.DataChanged += XTerminalDataChanged;
            YTerminal = setup.InputTerminal<int>("Y", Direction.West);
            YTerminal.DataChanged += YTerminalDataChanged;

            ColorTerminal = setup.InputTerminal<Color>("Color", Direction.West);
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

        private void XTerminalDataChanged(int x)
        {
            _x = x;
            _isCachedPositionValid = false;
            _isCachedElipseValid = false;

        }

        private void YTerminalDataChanged(int y)
        {
            _y = y;
            _isCachedPositionValid = false;
            _isCachedElipseValid = false;
        }

        private void WidthTerminalDataChanged(float width)
        {
            _width = width;
            _isCachedElipseValid = false;
        }

        private void HeightTerminalDataChanged(float height)
        {
            _height = height;
            _isCachedElipseValid = false;
        }
    }
}