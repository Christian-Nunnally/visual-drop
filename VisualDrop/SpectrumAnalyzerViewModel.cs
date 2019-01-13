using DiiagramrAPI.PluginNodeApi;
using Stylet;
using System.Diagnostics;
using System.Linq;

namespace VisualDrop
{
    public class SpectrumAnalyzerViewModel : PluginNode
    {
        public BindableCollection<int> Levels { get; set; } = new BindableCollection<int>();
        private int _lastNumberOfBars;
        private Stopwatch _refreshStopwatch = new Stopwatch();

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(90, 90);
            setup.NodeName("SpectrumAnalyzer");
            InputTerminal = setup.InputTerminal<byte[]>("Audio Data", Direction.North);
            setup.EnableResize();

            InputTerminal.DataChanged += InputTerminalOnDataChanged;

            _refreshStopwatch.Start();
        }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            if (_refreshStopwatch.ElapsedMilliseconds < 100)
            {
                return;
            }

            if (_lastNumberOfBars != data.Length)
            {
                _lastNumberOfBars = data.Length;
                BarWidth = (Width - 10) / _lastNumberOfBars;
            }
            Levels.Clear();
            Levels.AddRange(data.Select(x => (int)x).ToArray());

            _refreshStopwatch.Restart();
        }

        public Terminal<byte[]> InputTerminal { get; set; }

        public double BarWidth { get; set; } = 5;

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Width))
            {
                BarWidth = Width / _lastNumberOfBars;
            }
        }
    }
}
