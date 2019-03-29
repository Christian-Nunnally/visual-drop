using DiiagramrAPI.Diagram;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DiiagramrFadeCandy
{
    public class FadeCandyNodeViewModel : Node
    {
        private static bool FadeCandyConnected;
        private const int NumberOfDrivers = 8;
        private FadeCandyClient _fadeCandyClient;
        public bool ConnectButtonVisible { get; set; } = true;
        public LedChannelDriver[] _ledDrivers = new LedChannelDriver[NumberOfDrivers];
        public ObservableCollection<LedChannelDriver> EastDrivers { get; set; } = new ObservableCollection<LedChannelDriver>();
        public ObservableCollection<LedChannelDriver> WestDrivers { get; set; } = new ObservableCollection<LedChannelDriver>();
        public List<TypedTerminal<LedChannelDriver>> DriverTerminals { get; private set; } = new List<TypedTerminal<LedChannelDriver>>();
        public LedChannelDriver SelectedDriver { get; set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(150, 180);
            setup.NodeName("Fade Candy");

            for (int i = 0; i < NumberOfDrivers / 2; i++)
            {
                _ledDrivers[i] = new LedChannelDriver
                {
                    Box = new RawBox(0, 0, 8, 8),
                    Name = "pin " + i
                };
                var driverTerminal = setup.OutputTerminal<LedChannelDriver>("Led Driver " + i, Direction.West);
                WestDrivers.Add(_ledDrivers[i]);
                DriverTerminals.Add(driverTerminal);
                driverTerminal.Data = _ledDrivers[i];
            }

            for (int i = NumberOfDrivers - 1; i >= NumberOfDrivers / 2; i--)
            {
                _ledDrivers[i] = new LedChannelDriver
                {
                    Box = new RawBox(0, 0, 8, 8),
                    Name = "pin " + i
                };
                var driverTerminal = setup.OutputTerminal<LedChannelDriver>("Led Driver " + i, Direction.East);
                EastDrivers.Add(_ledDrivers[i]);
                DriverTerminals.Add(driverTerminal);
                driverTerminal.Data = _ledDrivers[i];
            }


            _ledDrivers[0].X = 8;
            _ledDrivers[7].X = 16;
            _ledDrivers[6].X = 24;
            SelectedDriver = _ledDrivers[0];
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

        public void DriverSelected(object param)
        {
            if (SelectedDriver == param)
            {
                SelectedDriver.IsSelected = !SelectedDriver.IsSelected;
                return;
            }
            SelectedDriver.IsSelected = false;
            SelectedDriver = (LedChannelDriver)param;
            SelectedDriver.IsSelected = true;
        }
    }
}
