using System;
using System.Windows;
using DiiagramrAPI.PluginNodeApi;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.WIC.Bitmap;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace DiiagramrFadeCandy
{
    public class DirectXNodeViewModel : PluginNode
    {
        private static readonly ImagingFactory wicFactory = new ImagingFactory();
        private static readonly SharpDX.Direct2D1.Factory d2dFactory = new SharpDX.Direct2D1.Factory();
        private static readonly PixelFormat pixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Unknown);
        private static readonly RenderTargetProperties renderTargetProperties = new RenderTargetProperties(RenderTargetType.Default, pixelFormat, 0, 0, RenderTargetUsage.None, FeatureLevel.Level_DEFAULT);
        private static readonly bool ClearBeforeFrame = false;
        private static readonly RawColor4 Black = new RawColor4(0, 0, 0, 1);
        private readonly LedChannelDriver[] _ledDrivers = new LedChannelDriver[8];
        private FadeCandyClient _fadeCandyClient = new FadeCandyClient("127.0.0.1", 7890, false, false);

        private Size _bitmapSize = new Size(8,8);
        public Size BitmapSize
        {
            get => _bitmapSize;
            set
            {
                _cachedBitmap = null;
                _bitmapSize = value;
            }
        }

        private Bitmap _cachedBitmap;
        private Bitmap Bitmap
        {
            get => _cachedBitmap ?? CreateAndCacheBitmap();
            set
            {
                if (value == null) RenderTarget = null;
            }
        }

        private Bitmap CreateAndCacheBitmap()
        {
            _cachedBitmap = new Bitmap(
                wicFactory,
                (int)_bitmapSize.Width,
                (int)_bitmapSize.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);
            return _cachedBitmap;
        }

        private WicRenderTarget _cachedRenderTarget;
        public WicRenderTarget RenderTarget
        {
            get => _cachedRenderTarget ?? CreateAndCacheRenderTarget();
            set => _cachedRenderTarget = value;
        }

        private WicRenderTarget CreateAndCacheRenderTarget()
        {
            _cachedRenderTarget = new WicRenderTarget(d2dFactory, Bitmap, renderTargetProperties);
            return _cachedRenderTarget;
        }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(120, 40);
            setup.NodeName("Direct X");

            _ledDrivers[0] = new LedChannelDriver();
            _ledDrivers[0].Box = new RawBox(0, 0, 8, 8);

            RenderFrame();
        }

        private void RenderFrame()
        {
            RenderTarget.BeginDraw();
            if (ClearBeforeFrame) RenderTarget.Clear(Black);

            RenderTarget.FillRectangle(new RawRectangleF(1, 2, 3, 4), new SolidColorBrush(RenderTarget, new RawColor4(0, 1, 0, 1)));

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
    }
}