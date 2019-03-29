﻿using DiiagramrAPI.Diagram;
using System;
using System.Threading;
using System.Windows;

namespace DiiagramrFadeCandy
{
    public class SineAnimationNodeViewModel : Node
    {
        private int _frames = 10;
        private readonly int _timeBetweenFrames = 30;
        private float _startPosition = 4;
        private float _quadrents = 2;

        public Point[] UIPoints { get; set; }
        public TypedTerminal<bool> TriggerTerminal { get; private set; }
        public TypedTerminal<float> AmplitudeTerminal { get; private set; }
        public TypedTerminal<float> ValueTerminal { get; private set; }
        public TypedTerminal<float> OffsetTerminal { get; private set; }
        public TypedTerminal<int> FramesTerminal { get; private set; }
        public TypedTerminal<float> QuadrentsTerminal { get; private set; }

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

            UIPoints = new Point[_frames];
            int frame = 0;
            for (double d = 0.0; d < _quadrents * (Math.PI / 2.0); d += _quadrents * (Math.PI / 2.0) / _frames)
            {
                if (frame == UIPoints.Length)
                {
                    break;
                }

                var x = frame * (Width / _frames);
                var y = Height / 2 * Math.Sin(d);
                UIPoints[frame] = new Point(x, y);
                frame++;
                OnPropertyChanged(nameof(UIPoints));
            }
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
