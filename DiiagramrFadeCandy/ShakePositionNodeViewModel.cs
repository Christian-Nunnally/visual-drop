using DiiagramrAPI.PluginNodeApi;
using System;
using System.Threading;

namespace DiiagramrFadeCandy
{
    public class ShakePositionNodeViewModel : PluginNode
    {
        private const int Frames = 10;
        private const int TimeBetweenFrames = 33;

        public Terminal<bool> TriggerTerminal { get; private set; }
        public Terminal<int> ShakeAmountInputTerminal { get; private set; }
        public Terminal<int> ValueOutputTerminal { get; private set; }

        public int StartingPosition { get; set; } = 4;

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("Shake");

            TriggerTerminal = setup.InputTerminal<bool>("Trigger", Direction.North);
            TriggerTerminal.DataChanged += TriggerTerminalDataChanged;

            ShakeAmountInputTerminal = setup.InputTerminal<int>("Amplitude", Direction.West);

            ValueOutputTerminal = setup.OutputTerminal<int>("Value", Direction.East);
            ValueOutputTerminal.Data = StartingPosition;
        }

        private void TriggerTerminalDataChanged(bool data)
        {
            if (data)
            {
                new Thread(() =>
                {
                    for (double d = 0; d < 2 * Math.PI; d += 2 * Math.PI / Frames)
                    {
                        ValueOutputTerminal.Data = (int)(StartingPosition + (ShakeAmountInputTerminal.Data * Math.Sin(d)));
                        Thread.Sleep(TimeBetweenFrames);
                    }
                    ValueOutputTerminal.Data = StartingPosition;
                }).Start();
            }
        }
    }
}