using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class AverageSpectrumViewModel : PluginNode
    {
        public float LastDataWeight { get; set; }
        private byte[] _lastData = new byte[0];

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 40);
            setup.NodeName("AverageSpectrum");

            InputTerminal = setup.InputTerminal<byte[]>("Spectrum In", Direction.North);
            OutputTerminal = setup.OutputTerminal<byte[]>("Spectrum Out", Direction.South);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private Terminal<byte[]> OutputTerminal { get; set; }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null) return;
            if (_lastData.Length != data.Length) _lastData = new byte[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                _lastData[i] = (byte) (LastDataWeight * _lastData[i] + data[i] * (1.0 - LastDataWeight));
            }

            OutputTerminal.Data = null;
            OutputTerminal.Data = _lastData;
        }

        public Terminal<byte[]> InputTerminal { get; set; }
    }
}
