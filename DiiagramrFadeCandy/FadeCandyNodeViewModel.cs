using DiiagramrAPI.Diagram;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace DiiagramrFadeCandy
{
    public class FadeCandyNodeViewModel : Node
    {
        private static bool FadeCandyConnected;
        private const int NumberOfDrivers = 8;
        private FadeCandyClient _fadeCandyClient;
        public bool ConnectButtonVisible { get; set; } = true;
        public LedChannelDriver[] _ledDrivers = new LedChannelDriver[NumberOfDrivers];
        public ObservableCollection<LedChannelDriver> Drivers { get; set; } = new ObservableCollection<LedChannelDriver>();
        public List<TypedTerminal<LedChannelDriver>> DriverTerminals { get; private set; } = new List<TypedTerminal<LedChannelDriver>>();
        public LedChannelDriver SelectedDriver { get; set; }
        public bool IsDriverSelected => SelectedDriver != null;
        public double DriverButtonWidthOnView { get; set; }
        public string ServerStatusString { get; set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(180, 180);
            setup.NodeName("Fade Candy");
            Drivers.CollectionChanged += Drivers_CollectionChanged;

            for (int i = 0; i < NumberOfDrivers; i++)
            {
                _ledDrivers[i] = new LedChannelDriver
                {
                    Box = new RawBox(0, 0, 8, 8),
                    Name = "pin " + i
                };
                var driverTerminal = setup.OutputTerminal<LedChannelDriver>("Led Driver " + i, Direction.North);
                Drivers.Add(_ledDrivers[i]);
                DriverTerminals.Add(driverTerminal);
                driverTerminal.Data = _ledDrivers[i];
            }
        }

        private void Drivers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DriverButtonWidthOnView = Drivers.Count == 0 ? 0 : ((Width - 10) / Drivers.Count) - 2;
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
                    _fadeCandyClient.PutPixels(_ledDrivers);
                    ServerStatusString = _fadeCandyClient.Status;
                }
            }).Start();
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

        public void SelectDriver(LedChannelDriver driver)
        {
            if (driver == null)
            {
                SelectedDriver.IsSelected = false;
                SelectedDriver = null;
                return;
            }
            else if (driver == SelectedDriver)
            {
                SelectDriver(null);
                return;
            }
            if (SelectedDriver != null)
            {
                SelectedDriver.IsSelected = false;
            }
            SelectedDriver = driver;
            SelectedDriver.IsSelected = true;
        }

        public void MouseEnterSourceButton(object sender, MouseEventArgs e)
        {
        }

        public void MouseLeaveSourceButton(object sender, MouseEventArgs e)
        {
        }

        public void MouseDownSourceButton(object sender, MouseEventArgs e)
        {
            var ledDriver = GetLedChannelDriverFromSender(sender);
            SelectDriver(ledDriver);
        }

        private LedChannelDriver GetLedChannelDriverFromSender(object sender)
        {
            return sender is FrameworkElement frameworkElement
                ? frameworkElement.DataContext as LedChannelDriver
                : null;
        }
    }
}
