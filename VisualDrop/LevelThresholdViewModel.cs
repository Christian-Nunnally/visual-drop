using DiiagramrAPI.PluginNodeApi;
using System.Windows.Media;

namespace VisualDrop
{
    public class LevelThresholdViewModel : PluginNode
    {
        public int Index { get; set; }

        private int _lastInputArrayLength = 0;

        private bool OnOffState
        {
            get => _onOffState;

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

        private int _onThreshold = 215;
        private int _offThreshold = 170;
        private bool _onOffState;

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(60, 60);
            setup.NodeName("Threshold");
            InputTerminal = setup.InputTerminal<byte[]>("Input", Direction.North);
            OutputTerminal = setup.OutputTerminal<bool>("On/Off", Direction.South);
            IndexTerminal = setup.InputTerminal<int>("Index", Direction.West);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
            IndexTerminal.DataChanged += IndexTerminalOnDataChanged;
        }

        private void IndexTerminalOnDataChanged(int data)
        {
            Index = data;
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
                Index = _lastInputArrayLength - 1;
                return;
            }
            if (Index < 0)
            {
                Index = 0;
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
        public Terminal<int> IndexTerminal { get; private set; }
        public int InputValue { get; set; }

        [PluginNodeSetting]
        public int OnThreshold
        {
            get => _onThreshold;

            set
            {
                _onThreshold = value;
                if (OffThreshold > OnThreshold)
                {
                    OffThreshold = OnThreshold;
                }
            }
        }

        [PluginNodeSetting]
        public int OffThreshold
        {
            get => _offThreshold;

            set
            {
                _offThreshold = value;
                if (OnThreshold < OffThreshold)
                {
                    OnThreshold = OffThreshold;
                }
            }
        }

        public Brush ProgressBarForegroundColor { get; set; }

        public void ProgressBarDoubleClicked()
        {
            InvertOutput = !InvertOutput;
            OnOffState = !OnOffState;
        }
    }
}
