using DiiagramrAPI.PluginNodeApi;
using System;
using System.Threading;

namespace DiiagramrFadeCandy
{
    public class SineAnimationNodeViewModel : PluginNode
    {
        private int _frames = 10;
        private readonly int _timeBetweenFrames = 30;
        private float _startPosition = 4;
        private float _quadrents = 2;

        public Terminal<bool> TriggerTerminal { get; private set; }
        public Terminal<float> AmplitudeTerminal { get; private set; }
        public Terminal<float> ValueTerminal { get; private set; }
        public Terminal<float> OffsetTerminal { get; private set; }
        public Terminal<int> FramesTerminal { get; private set; }
        public Terminal<float> QuadrentsTerminal { get; private set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(90, 90);
            setup.NodeName("Sine");

            TriggerTerminal = setup.InputTerminal<bool>("Trigger", Direction.North);
            TriggerTerminal.DataChanged += TriggerTerminalDataChanged;

            AmplitudeTerminal = setup.InputTerminal<float>("Amplitude", Direction.West);
            AmplitudeTerminal.Data = 1;

            ValueTerminal = setup.OutputTerminal<float>("Value", Direction.East);
            ValueTerminal.Data = _startPosition;

            OffsetTerminal = setup.InputTerminal<float>("Offset", Direction.West);
            OffsetTerminal.Data = _startPosition;
            OffsetTerminal.DataChanged += OffsetTerminalDataChanged;

            FramesTerminal = setup.InputTerminal<int>("Frames", Direction.North);
            FramesTerminal.Data = _frames;
            FramesTerminal.DataChanged += FramesTerminalDataChanged;

            QuadrentsTerminal = setup.InputTerminal<float>("Quadrents", Direction.South);
            QuadrentsTerminal.Data = _quadrents;
            QuadrentsTerminal.DataChanged += QuadrentsTerminalDataChanged;
        }

        private void QuadrentsTerminalDataChanged(float quadrents)
        {
            _quadrents = quadrents;
        }

        private void FramesTerminalDataChanged(int frames)
        {
            _frames = frames;
        }

        private void OffsetTerminalDataChanged(float offset)
        {
            _startPosition = offset;
        }

        private void TriggerTerminalDataChanged(bool data)
        {
            if (data)
            {
                new Thread(() =>
                {
                    for (double d = 0.0; d < _quadrents * (Math.PI / 2.0); d += _quadrents * (Math.PI / 2.0) / _frames)
                    {
                        ValueTerminal.Data = (float)(_startPosition + (AmplitudeTerminal.Data * Math.Sin(d)));
                        Thread.Sleep(_timeBetweenFrames);
                    }
                    ValueTerminal.Data = _startPosition;
                }).Start();
            }
        }
    }
}
