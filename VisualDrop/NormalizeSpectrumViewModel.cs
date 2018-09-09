using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class NormalizeSpectrumViewModel : PluginNode
    {
        private double[] _max = new double[1];
        private double[] _min = new double[1];
        private byte[] _outputData = new byte[1];

        public double ReturnSpeed { get; set; } = 0.01;
        public double MaxRange { get; set; } = 10;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(120, 80);
            setup.NodeName("NormalizeSpectrum");
            SpectrumInputTerminal = setup.InputTerminal<byte[]>("Spectrum In", Direction.North);
            SpectrumOutputTerminal = setup.OutputTerminal<byte[]>("Normalized Out", Direction.South);

            SpectrumInputTerminal.DataChanged += SpectrumInputTerminalOnDataChanged;
        }

        private void SpectrumInputTerminalOnDataChanged(byte[] data)
        {
            if (data == null || data.Length == 0) return;
            if (_max.Length != data.Length)
            {
                _max = new double[data.Length];
                _min = new double[data.Length];
                _outputData = new byte[data.Length];
            }

            for (var i = 0; i < data.Length; i++)
            {
                var currentRange = _max[i] - _min[i];
                var adjustmentAmount = currentRange * ReturnSpeed;
                _max[i] -= adjustmentAmount;
                _min[i] += adjustmentAmount;
                if (data[i] > _max[i]) _max[i] = data[i];
                if (data[i] < _min[i]) _min[i] = data[i];
                if (_max[i] - MaxRange < _min[i]) _min[i] = _max[i] - MaxRange;
                _min[i] = Math.Max(0, _min[i]);
                _max[i] = Math.Min(255, _max[i]);
                _outputData[i] = (byte) ((data[i] - _min[i]) * (255 / Math.Max(1, _max[i] - _min[i])));
            }

            SpectrumOutputTerminal.Data = null;
            SpectrumOutputTerminal.Data = _outputData;
        }

        public void SubtractMaxRange()
        {
            if (MaxRange > 2) MaxRange -= 1;
        }

        public void AddMaxRange()
        {
            if (MaxRange < 100) MaxRange += 1;
        }

        public void SubtractReturnSpeed()
        {
            if (ReturnSpeed > 0.001) ReturnSpeed -= 0.001;
        }

        public void AddReturnSpeed()
        {
            ReturnSpeed += 0.001;
        }

        public Terminal<byte[]> SpectrumOutputTerminal { get; set; }

        public Terminal<byte[]> SpectrumInputTerminal { get; set; }
    }
}
