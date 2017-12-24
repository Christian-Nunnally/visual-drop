using System;
using System.Collections.Generic;
using System.Linq;
using DiiagramrAPI.PluginNodeApi;
using Stylet;

namespace VisualDrop
{
    public class SpectrumAnalyzerViewModel : PluginNode
    {
        public BindableCollection<int> Levels { get; set; } = new BindableCollection<int>();
        private int _lastNumberOfBars;
        private long _lastDisplayUpdateTimeInMilliseconds;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("SpectrumAnalyzer");
            InputTerminal = setup.InputTerminal<byte[]>("Audio Data", Direction.North);
            setup.EnableResize();

            InputTerminal.DataChanged += InputTerminalOnDataChanged;

            Levels.Add(150);
            Levels.Add(75);
        }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null) return;
            if (DateTime.Now.Ticks / 10000 - _lastDisplayUpdateTimeInMilliseconds <= 33) return;
            _lastDisplayUpdateTimeInMilliseconds = DateTime.Now.Ticks / 10000;

            if (_lastNumberOfBars != data.Length)
            {
                _lastNumberOfBars = data.Length;
                BarWidth = (Width - 10) / _lastNumberOfBars;
            }
            Levels.Clear();
            Levels.AddRange(data.Select(x => (int)x).ToArray());
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
