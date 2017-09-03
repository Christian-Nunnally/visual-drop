using DiagramEditor.Model;
using DiagramEditor.ViewModel.Diagram;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace ColorOrgan5Nodes.Nodes
{
    public class ThresholdNodeViewModel : PluginNodeViewModel
    {
        private const string DataInDelegateKey = "DataIn";
        private float _threshold;
        private OutputTerminalViewModel _outputTerminal;

        public override string Name => "Threshold Node";

        public override void NodeLoaded()
        {
            Maximum = 10;
            LastValue = 0;
            DecayRate = 1;
        }

        private IDictionary<OutputTerminal, object> DataIn(object o)
        {
            var dataOrNull = o as byte?;
            if (dataOrNull == null) return null;
            var data = (int)dataOrNull;

            Maximum -= DecayRate;
            Maximum = Math.Max(Maximum, data);

            Minimum += DecayRate;
            Minimum = Math.Min(Minimum, data);

            LastValue = data;

            return ComputeOutput();
        }

        public override void ConstructTerminals()
        {
            ConstructNewInputTerminal("Input Signal", typeof(int), Direction.North, DataInDelegateKey);
            _outputTerminal = ConstructNewOutputTerminal("Trigger", typeof(bool), Direction.South);
        }

        public override void SetupDelegates(DelegateMapper delegateMapper)
        {
            delegateMapper.AddMapping(DataInDelegateKey, DataIn);
        }

        private Dictionary<OutputTerminal, object> ComputeOutput()
        {
            var output = new Dictionary<OutputTerminal, object>();

            var currentGain = (LastValue - Minimum) / (Maximum - Minimum);
            if (currentGain > ThresholdPrecent / 100)
            {
                output.Add(_outputTerminal.OutputTerminal, true);
                OutputIndicatorColor = new SolidColorBrush(Colors.LightSlateGray);
            }
            else
            {
                output.Add(_outputTerminal.OutputTerminal, false);
                OutputIndicatorColor = new SolidColorBrush(Colors.DarkSlateGray);
            }

            return output;
        }

        public float DecayRate { get; set; }

        public float LastValue { get; set; }

        public float Maximum { get; set; }

        public float Minimum { get; set; }

        public float ThresholdPrecent { get; set; }

        public SolidColorBrush OutputIndicatorColor { get; set; }
    }
}
