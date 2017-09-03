using DiagramEditor.Model;
using DiagramEditor.ViewModel.Diagram;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ColorOrgan5Nodes.Nodes
{
    [Serializable]
    public class SpectrumAnalyzerNodeViewModel : PluginNodeViewModel
    {
        private const string AudioDataInDelegateKey = "AudioDataIn";
        private int _spectrumWidth;

        public override string Name => "Spectrum Analyzer";

        private IList<OutputTerminalViewModel> OrderedOutputTerminals { get; set; }
        
        public BindableCollection<int> Levels { get; set; }

        public override void NodeLoaded()
        {
            Levels = new BindableCollection<int>();
            OrderedOutputTerminals = new List<OutputTerminalViewModel>();

            var spectrumWidth = LoadValue(nameof(SpectrumWidth));
            SpectrumWidth = (int)(spectrumWidth ?? SpectrumWidth);
        }

        public override void NodeSaved()
        {
            SaveValue(nameof(SpectrumWidth), SpectrumWidth);
        }

        public override void ConstructTerminals()
        {
            ConstructNewInputTerminal("Audio In", typeof(List<byte>), Direction.North, AudioDataInDelegateKey);
        }

        public override void SetupDelegates(DelegateMapper delegateMapper)
        {
            delegateMapper.AddMapping(AudioDataInDelegateKey, AudioDataIn);
        }

        public int SpectrumWidth
        {
            get { return _spectrumWidth; }
            set
            {
                if (_spectrumWidth == value) return;
                _spectrumWidth = value;
                UpdateAllTerminalPositions();
            }
        }

        private IDictionary<OutputTerminal, object> AudioDataIn(object data)
        {
            var audioData = (List<byte>)data;
            MakeOutputForEachDatum(audioData.Count);
            SetVisualizer(audioData);
            return ComputeOutputTerminalMapping(audioData);
        }

        private IDictionary<OutputTerminal, object> ComputeOutputTerminalMapping(List<byte> audioData)
        {
            var outputMap = new Dictionary<OutputTerminal, object>();
            for (var i = 0; i < audioData.Count; i++) outputMap.Add(OrderedOutputTerminals[i].OutputTerminal, audioData[i]);
            return outputMap;
        }

        private void MakeOutputForEachDatum(int numberOfDatum)
        {
            if (OrderedOutputTerminals.Count != OutputTerminalViewModels.Count())
            {
                foreach (var outputTerminalViewModel in OutputTerminalViewModels.Where(x => !OrderedOutputTerminals.Contains(x)))
                {
                    OrderedOutputTerminals.Add(outputTerminalViewModel);
                }
            }

            while (OutputTerminalViewModels.Count() < numberOfDatum) OrderedOutputTerminals.Add(ConstructNewOutputTerminal(OrderedOutputTerminals.Count.ToString(), typeof(int), Direction.South));
            while (OutputTerminalViewModels.Count() > numberOfDatum)
            {
                RemoveTerminalViewModel(OrderedOutputTerminals.First());
                OrderedOutputTerminals.RemoveAt(0);
            }
        }

        private void SetVisualizer(IEnumerable<byte> data)
        {
            Levels.Clear();
            Levels.AddRange(data.Select(x => (int)x).ToArray());
        }
    }
}