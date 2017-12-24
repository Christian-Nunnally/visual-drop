using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class AudioSourceViewModel : PluginNode
    {
        public ObservableCollection<string> Sources { get; set; } = new ObservableCollection<string>();
        public AudioSourceAnalyzer AudioSourceAnalyzer { get; set; }

        public Terminal<byte[]> AudioOutputTerminal { get; set; } 

        public string SelectedDevice { get; set; }

        public bool Enabled { get; set; }
        public string ToggleEnableButtonText => Enabled ? "Disable" : "Enable";

        public SolidColorBrush EnabledIndicatorColor => Enabled ? Brushes.LightGreen : Brushes.LightCoral;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(200, 60);
            setup.NodeName("AudioSource");
            AudioOutputTerminal = setup.OutputTerminal<byte[]>("Audio Data", Direction.South);
            NumberOfLinesTerminal = setup.InputTerminal<int>("Number of Lines", Direction.West);

            NumberOfLinesTerminal.DataChanged += NumberOfLinesTerminalOnDataChanged;

            AudioSourceAnalyzer = new AudioSourceAnalyzer();
            AudioSourceAnalyzer.Lines = 32;
            AudioSourceAnalyzer.AudioDataReceived += AudioSourceAnalyzerOnAudioDataReceived;
            foreach (var deviceString in AudioSourceAnalyzer.GetDeviceList())
                Sources.Add(deviceString);
            SelectedDevice = Sources.LastOrDefault();
        }

        private void NumberOfLinesTerminalOnDataChanged(int data)
        {
            if (data <= 0 || data > 100) return;
            AudioSourceAnalyzer.Lines = data;
        }

        public Terminal<int> NumberOfLinesTerminal { get; set; }

        private void AudioSourceAnalyzerOnAudioDataReceived(List<byte> bytes)
        {
            AudioOutputTerminal.Data = null;
            AudioOutputTerminal.Data = bytes.ToArray();
        }

        public void ToggleEnable()
        {
            if (Enabled) AudioSourceAnalyzer.Disable();
            else AudioSourceAnalyzer.Enable(SelectedDevice);
            Enabled = !Enabled;
        }
    }
}