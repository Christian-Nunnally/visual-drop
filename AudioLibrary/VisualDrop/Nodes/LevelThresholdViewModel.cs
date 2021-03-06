using DiiagramrAPI.Diagram;
using System.Windows.Media;

namespace VisualDrop
{
    public class LevelThresholdViewModel : Node
    {
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
            InputTerminal = setup.InputTerminal<float>("Input", Direction.North);
            OutputTerminal = setup.OutputTerminal<bool>("On/Off", Direction.South);

            InputTerminal.DataChanged += InputTerminalOnDataChanged;
        }

        private void InputTerminalOnDataChanged(float value)
        {
            InputValue = value;
            if (value > MaxValue)
            {
                MaxValue = value;
            }

            if (OnOffState && value < OffThreshold)
            {
                OnOffState = InvertOutput;
            }
            else if (!OnOffState && value >= OnThreshold)
            {
                OnOffState = !InvertOutput;
            }
        }

        public TypedTerminal<float> InputTerminal { get; set; }
        public TypedTerminal<bool> OutputTerminal { get; set; }
        public float InputValue { get; set; }

        [NodeSetting]
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

        [NodeSetting]
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
        public float MaxValue { get; private set; }

        public void ProgressBarDoubleClicked()
        {
            InvertOutput = !InvertOutput;
            OnOffState = !OnOffState;
        }
    }
}
