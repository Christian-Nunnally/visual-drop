using DiiagramrAPI.Diagram;
using System;

namespace VisualDrop
{
    public class NormalizeSpectrumViewModel : Node
    {
        private double[] _max = new double[1];
        private byte[] _outputData = new byte[1];

        public double ReturnSpeed { get; set; } = 0.01;
        public string ReturnSpeedString => "Decay = " + ReturnSpeed.ToString("0.00");
        public int MinimumPeak { get; set; } = 10;
        public string MinimumPeakString => "Minimun Peak = " + MinimumPeak.ToString("0");

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(90, 60);
            setup.NodeName("Log Decay");
            SpectrumInputTerminal = setup.InputTerminal<byte[]>("Data In", Direction.North);
            SpectrumOutputTerminal = setup.OutputTerminal<byte[]>("Normalized Data Out", Direction.South);

            SpectrumInputTerminal.DataChanged += SpectrumInputTerminalOnDataChanged;
        }

        private void SpectrumInputTerminalOnDataChanged(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            if (_max.Length != data.Length)
            {
                _max = new double[data.Length];
                _outputData = new byte[data.Length];
            }

            for (var i = 0; i < data.Length; i++)
            {
                var adjustmentAmount = Math.Log(data[i]) * ReturnSpeed;
                _max[i] -= adjustmentAmount;
                if (_max[i] < MinimumPeak)
                {
                    _max[i] = MinimumPeak;
                }
                if (data[i] > _max[i])
                {
                    _max[i] = data[i];
                }

                _max[i] = Math.Min(255, _max[i]);
                _outputData[i] = (byte)(data[i] * (255 / Math.Max(1, _max[i])));
            }

            SpectrumOutputTerminal.Data = null;
            SpectrumOutputTerminal.Data = _outputData;
        }

        public void SubtractMaxRange()
        {
            if (MinimumPeak > 2)
            {
                MinimumPeak -= 1;
            }
        }

        public void AddMaxRange()
        {
            if (MinimumPeak < 100)
            {
                MinimumPeak += 1;
            }
        }

        public void SubtractReturnSpeed()
        {
            if (ReturnSpeed > 0.001)
            {
                ReturnSpeed -= 0.001;
            }
        }

        public void AddReturnSpeed()
        {
            ReturnSpeed += 0.001;
        }

        public TypedTerminal<byte[]> SpectrumOutputTerminal { get; set; }

        public TypedTerminal<byte[]> SpectrumInputTerminal { get; set; }
    }
}
