using System;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class LevelThresholdViewModel : PluginNode
    {
        public int Index { get; set; }

        private int _lastInputArrayLength = 0;

        private bool OnOffState
        {
            get { return _onOffState; }
            set
            {
                OutputTerminal.Data = value;
                _onOffState = value;
                if (value)
                {
                    ProgressBarForegroundColor = InvertOutput ? Brushes.DarkSlateGray : Brushes.LightSlateGray;
                }
                else
                {
                    ProgressBarForegroundColor = InvertOutput ? Brushes.LightSlateGray : Brushes.DarkSlateGray;
                }
            }
        }

        public bool InvertOutput { get; set; }

        private int _onThreshold;
        private int _offThreshold;
        private bool _onOffState;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("LevelThreshold");
            InputTerminal = setup.InputTerminal<byte[]>("Input", Direction.North); 
            OutputTerminal = setup.OutputTerminal<bool>("On/Off", Direction.South); 

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private void InputTerminalOnDataChanged(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            _lastInputArrayLength = data.Length;
            if (Index >= data.Length)
            {
                Index = _lastInputArrayLength;
                return;
            }

            var byteData = data[Index];
            InputValue = byteData;
            if (OnOffState && byteData < OffThreshold)
            {
                OnOffState = false;
            }
            else if (!OnOffState && byteData >= OnThreshold)
            {
                OnOffState = true;
            }
        }

        public Terminal<byte[]> InputTerminal { get; set; }

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

        public Brush ProgressBarForegroundColor { get; set; }

        public void ProgressBarDoubleClicked()
        {
            InvertOutput = !InvertOutput;
            OnOffState = !OnOffState;
            OnOffState = !OnOffState;
        }

        public void Add()
        {
            if (Index < _lastInputArrayLength - 1) Index++;
        }

        public void Subtract()
        {
            if (Index > 0) Index--;
        }
    }
}
