using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class SpectrumRateOfChangeViewModel : PluginNode
    {
        public float LastDataWeight { get; set; }
        private byte[] _lastData = new byte[0];
        private byte[] _outputData = new byte[0];

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 40);
            setup.NodeName("Rate of Change");

            InputTerminal = setup.InputTerminal<byte[]>("Spectrum In", Direction.North);
            OutputTerminal = setup.OutputTerminal<byte[]>("Spectrum Out", Direction.South);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private Terminal<byte[]> OutputTerminal { get; set; }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            if (_lastData.Length == data.Length)
            {
                if (_outputData.Length != data.Length)
                {
                    _outputData = new byte[data.Length];
                }

                for (var i = 0; i < data.Length; i++)
                {
                    _outputData[i] = (byte)((_lastData[i] - data[i] + 128) / 2);
                }

                OutputTerminal.Data = null;
                OutputTerminal.Data = _outputData;
            }

            _lastData = data;
        }

        public Terminal<byte[]> InputTerminal { get; set; }
    }
}
