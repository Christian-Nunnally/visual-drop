using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DiiagramrAPI.PluginNodeApi;
using Stylet;

namespace VisualDrop
{
    public class SpectrumAnalyzerViewModel : PluginNode
    {
        public BindableCollection<int> Levels { get; set; } = new BindableCollection<int>();

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("SpectrumAnalyzer");
            InputTerminal = setup.InputTerminal<List<byte>>("Audio Data", Direction.North);
            setup.EnableResize();

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private void InputTerminalOnDataChanged(List<byte> data)
        {
            if (data == null) return;
            Levels.Clear();
            Levels.AddRange(data.Select(x => (int)x).ToArray());
        }

        public Terminal<List<byte>> InputTerminal { get; set; }
    }
}
