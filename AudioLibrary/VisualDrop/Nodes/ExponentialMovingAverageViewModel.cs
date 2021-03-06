using DiiagramrAPI.Diagram;

namespace VisualDrop
{
    public class ExponentialMovingAverageViewModel : Node
    {
        public float LastDataWeight { get; set; }
        public string WeightString => "Weight = " + LastDataWeight.ToString("0.00");
        private byte[] _lastData = new byte[0];

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(90, 30);
            setup.NodeName("Moving Average (EMA)");

            InputTerminal = setup.InputTerminal<byte[]>("Data In", Direction.North);
            OutputTerminal = setup.OutputTerminal<byte[]>("Data Out", Direction.South);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private TypedTerminal<byte[]> OutputTerminal { get; set; }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            if (_lastData.Length != data.Length)
            {
                _lastData = new byte[data.Length];
            }

            for (var i = 0; i < data.Length; i++)
            {
                _lastData[i] = (byte)((data[i] * LastDataWeight) + (_lastData[i] * (1.0 - LastDataWeight)));
            }

            OutputTerminal.Data = null;
            OutputTerminal.Data = _lastData;
        }

        public TypedTerminal<byte[]> InputTerminal { get; set; }
    }
}
