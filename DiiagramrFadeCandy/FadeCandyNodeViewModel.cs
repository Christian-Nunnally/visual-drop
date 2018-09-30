using DiiagramrAPI.PluginNodeApi;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.WIC.Bitmap;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace DiiagramrFadeCandy
{
    public class FadeCandyNodeViewModel : PluginNode
    {
        private static readonly ImagingFactory wicFactory = new ImagingFactory();
        private static readonly SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        private static readonly PixelFormat pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm_SRgb, AlphaMode.Unknown);
        private static readonly RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, pixelFormat, 0, 0, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
        private static readonly bool ClearBeforeFrame = true;
        private static readonly RawColor4 Black = new RawColor4(0, 0, 0, 1);
        private readonly LedChannelDriver[] _ledDrivers = new LedChannelDriver[8];
        private FadeCandyClient _fadeCandyClient;

        private List<IGraphicEffect> _graphicEffects = new List<IGraphicEffect>();

        public bool ConnectButtonVisible { get; set; } = true;

        private static bool FadeCandyConnected;
        private int _bitmapWidth = 8;
        private int _bitmapHeight = 8;

        [PluginNodeSetting]
        public int BitmapWidth
        {
            get => _bitmapWidth;
            set
            {
                _bitmapWidth = value;
                BitmapSize = new Size(_bitmapWidth, _bitmapHeight);
            }

        }

        [PluginNodeSetting]
        public int BitmapHeight
        {
            get => _bitmapHeight;
            set
            {
                _bitmapHeight = value;
                BitmapSize = new Size(_bitmapWidth, _bitmapHeight);
            }

        }

        private Size _bitmapSize = new Size(8, 8);
        public Size BitmapSize
        {
            get => _bitmapSize;
            set
            {
                _bitmapSize = value;
                Bitmap = null;
            }
        }

        private Bitmap _cachedBitmap;
        private Bitmap Bitmap
        {
            get => _cachedBitmap ?? CreateAndCacheBitmap();
            set
            {
                if (value == null)
                {
                    _cachedRenderTarget = null;
                }

                _cachedBitmap = value;
            }
        }

        private Bitmap CreateAndCacheBitmap()
        {
            _cachedBitmap = new Bitmap(
                wicFactory,
                _bitmapSize.Width < 1 ? 1 : (int)_bitmapSize.Width,
                _bitmapSize.Height < 1 ? 1 : (int)_bitmapSize.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);
            _cachedRenderTarget = null;
            return _cachedBitmap;
        }

        private WicRenderTarget _cachedRenderTarget;
        public WicRenderTarget RenderTarget => _cachedRenderTarget ?? CreateAndCacheRenderTarget();

        public Terminal<IGraphicEffect> EffectsTerminal { get; private set; }
        public Terminal<int> BitmapWidthTerminal { get; private set; }
        public Terminal<int> BitmapHeightTerminal { get; private set; }

        private WicRenderTarget CreateAndCacheRenderTarget()
        {
            _cachedRenderTarget = new WicRenderTarget(d2dFactory, Bitmap, renderTargetProperties);
            return _cachedRenderTarget;
        }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(120, 40);
            setup.NodeName("FadeCandy");

            EffectsTerminal = setup.InputTerminal<IGraphicEffect>("Effects", Direction.North);
            EffectsTerminal.DataChanged += EffectsTerminalDataChanged;

            BitmapWidthTerminal = setup.InputTerminal<int>("Width", Direction.West);
            BitmapHeightTerminal = setup.InputTerminal<int>("Height", Direction.West);
            BitmapWidthTerminal.DataChanged += BitmapWidthTerminalDataChanged;
            BitmapWidthTerminal.Data = _bitmapWidth;

            BitmapHeightTerminal.DataChanged += BitmapHeightTerminalDataChanged;
            BitmapHeightTerminal.Data = _bitmapHeight;

            for (int i = 0; i < _ledDrivers.Length; i++)
            {
                _ledDrivers[i] = new LedChannelDriver
                {
                    Box = new RawBox(0, 0, 8, 8)
                };
            }
        }

        private void BitmapWidthTerminalDataChanged(int width)
        {
            BitmapWidth = width;
        }

        private void BitmapHeightTerminalDataChanged(int height)
        {
            BitmapHeight = height;
        }

        private void EffectsTerminalDataChanged(IGraphicEffect data)
        {
            if (data == null)
            {
                return;
            }

            _graphicEffects.Add(data);
        }

        private void RenderFrame()
        {
            RenderTarget.BeginDraw();
            if (ClearBeforeFrame)
            {
                RenderTarget.Clear(Black);
            }

            foreach (var effect in _graphicEffects)
            {
                effect.Draw(RenderTarget);
            }

            RenderTarget.EndDraw();

            _fadeCandyClient.PutPixels(Bitmap, _ledDrivers);
        }

        private void SharpDxTest()
        {
            var wicFactory = new ImagingFactory();
            var d2dFactory = new SharpDX.Direct2D1.Factory();
            const int width = 5;
            const int height = 5;

            var rectangleGeometry = new RoundedRectangleGeometry(d2dFactory, new RoundedRectangle() { RadiusX = 1, RadiusY = 1, Rect = new RawRectangleF(1, 1, 3, 3) });
            var wicBitmap = new Bitmap(wicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppBGR, BitmapCreateCacheOption.CacheOnLoad);
            var renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, new PixelFormat(Format.Unknown, AlphaMode.Unknown), 0, 0, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
            var d2dRenderTarget = new WicRenderTarget(d2dFactory, wicBitmap, renderTargetProperties);
            var solidColorBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(1, 1, 2, 1));

            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.Clear(new RawColor4(0, 0, 0, 1));
            d2dRenderTarget.FillGeometry(rectangleGeometry, solidColorBrush, null);
            d2dRenderTarget.EndDraw();

            var buffer = new int[width * height];
            wicBitmap.CopyPixels(buffer);
            // var buffer = new int[4];
            // wicBitmap.CopyPixels(new RawBox(0, 0, 2, 2), buffer);


            var s = "";
            foreach (var e in buffer)
            {
                s += $"{e.ToString("X4")}, ";
            }
        }

        public void ConnectFadeCandy()
        {
            ConnectButtonVisible = false;
            OnPropertyChanged(nameof(ConnectButtonVisible));
            _fadeCandyClient = new FadeCandyClient("127.0.0.1", 7890, false, false);

            if (FadeCandyConnected)
            {
                return;
            }

            FadeCandyConnected = true;
            new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(33);
                    RenderFrame();
                }
            }).Start();
        }
    }
}