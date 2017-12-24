using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class LevelThresholdViewModel : PluginNode
    {
        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("LevelThreshold");
            InputTerminal = setup.InputTerminal<byte>("Input", Direction.North);
            OutputTerminal = setup.OutputTerminal<bool>("On/Off", Direction.South);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private void InputTerminalOnDataChanged(byte data) 
        {
            OutputTerminal.Data = data > 150;
        }

        public Terminal<byte> InputTerminal { get; set; }

        public Terminal<bool> OutputTerminal { get; set; }
    }
}
