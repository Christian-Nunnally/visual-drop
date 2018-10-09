using DiiagramrAPI.PluginNodeApi;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        public ObservableCollection<LedChannelDriver> Drivers { get; set; } = new ObservableCollection<LedChannelDriver>();

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
                _cachedBitmap = value;
                _cachedRenderTarget = null;
            }
        }

        private Bitmap CreateAndCacheBitmap()
        {
            Bitmap = new Bitmap(
                wicFactory,
                _bitmapSize.Width < 1 ? 1 : (int)_bitmapSize.Width,
                _bitmapSize.Height < 1 ? 1 : (int)_bitmapSize.Height,
                SharpDX.WIC.PixelFormat.Format32bppBGR,
                BitmapCreateCacheOption.CacheOnLoad);
            return Bitmap;
        }

        private WicRenderTarget _cachedRenderTarget;
        public WicRenderTarget RenderTarget => _cachedRenderTarget ?? CreateAndCacheRenderTarget();

        public Terminal<IGraphicEffect> EffectsTerminal { get; private set; }
        public Terminal<int> BitmapWidthTerminal { get; private set; }
        public Terminal<int> BitmapHeightTerminal { get; private set; }

        public LedChannelDriver SelectedDriver { get; private set; }
        public bool IsDriverSelected => SelectedDriver != null;

        private WicRenderTarget CreateAndCacheRenderTarget()
        {
            _cachedRenderTarget = new WicRenderTarget(d2dFactory, Bitmap, renderTargetProperties);
            return _cachedRenderTarget;
        }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(160, 160);
            setup.NodeName("FadeCandy");

            EffectsTerminal = setup.InputTerminal<IGraphicEffect>("Effects", Direction.North);
            EffectsTerminal.DataChanged += EffectsTerminalDataChanged;

            BitmapWidthTerminal = setup.InputTerminal<int>("Width", Direction.West);
            BitmapWidthTerminal.DataChanged += BitmapWidthTerminalDataChanged;
            BitmapWidthTerminal.Data = 64;
            BitmapHeightTerminal = setup.InputTerminal<int>("Height", Direction.West);
            BitmapHeightTerminal.DataChanged += BitmapHeightTerminalDataChanged;
            BitmapHeightTerminal.Data = 64;


            for (int i = 0; i < _ledDrivers.Length; i++)
            {
                _ledDrivers[i] = new LedChannelDriver
                {
                    Box = new RawBox(0, 0, 8, 8),
                    Name = "pin " + i
                };
                Drivers.Add(_ledDrivers[i]);
            }

            SelectedDriver = Drivers.First();
        }

        private void BitmapWidthTerminalDataChanged(int width)
        {
            if (width != 0)
            {
                BitmapWidth = width;
            }
        }

        private void BitmapHeightTerminalDataChanged(int height)
        {
            if (height != 0)
            {
                BitmapHeight = height;
            }
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

        public void ConnectFadeCandy()
        {
            ConnectButtonVisible = false;
            OnPropertyChanged(nameof(ConnectButtonVisible));

            OpenOrRestartFadeCandyServer();

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

        public void DriverSelected(object param)
        {
            SelectedDriver = (LedChannelDriver)param;
        }

        private void OpenOrRestartFadeCandyServer()
        {
            KillProcess("fcserver");
            StartProcess("fcserver.exe", "fcserver.exe");
        }

        private void KillProcess(string processName)
        {
            var processes = Process.GetProcesses().Where(p => p.ProcessName.Contains(processName));
            foreach (var process in processes)
            {
                process.Kill();
            }
        }

        private void StartProcess(string processName, string argument)
        {
            var enviromentPath = System.Environment.GetEnvironmentVariable("PATH");

            var paths = enviromentPath.Split(';');
            var exePath = paths.Select(x => Path.Combine(x, processName))
                               .Where(x => File.Exists(x))
                               .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(exePath) == false)
            {
                StartProcessInBackground(exePath);
            }
        }

        private void StartProcessInBackground(string exePath)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo(exePath)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            }.Start();
        }
    }
}