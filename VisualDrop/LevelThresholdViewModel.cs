using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class LevelThresholdViewModel : PluginNode
    {
        private bool _onOffState = false;
        private int _onThreshold;
        private int _offThreshold;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("LevelThreshold");
            InputTerminal = setup.InputTerminal<byte>("Input", Direction.North); 
            OutputTerminal = setup.OutputTerminal<bool>("On/Off", Direction.South); 

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private void InputTerminalOnDataChanged(byte data)
        {
            InputValue = data;
            if (_onOffState && data < OffThreshold)
            {
                _onOffState = false;
                OutputTerminal.Data = _onOffState;
            }
            else if (!_onOffState && data >= OnThreshold)
            {
                _onOffState = true;
                OutputTerminal.Data = _onOffState;
            }
        }

        public Terminal<byte> InputTerminal { get; set; }

        public Terminal<bool> OutputTerminal { get; set; }

        public int InputValue { get; set; }

        [PluginNodeSetting]
        public int OnThreshold
        {
            get { return _onThreshold; }
            set
            {
                _onThreshold = value;
                if (OffThreshold > OnThreshold) OffThreshold = OnThreshold;
            }
        }

        [PluginNodeSetting]
        public int OffThreshold
        {
            get { return _offThreshold; }
            set
            {
                _offThreshold = value;
                if (OnThreshold < OffThreshold) OnThreshold = OffThreshold;
            }
        }
    }
}
