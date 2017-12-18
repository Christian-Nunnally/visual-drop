using System.Collections.Generic;
using System.Collections.ObjectModel;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class AudioSourceViewModel : PluginNode
    {
        public ObservableCollection<string> Sources { get; set; } = new ObservableCollection<string>();
        public AudioSourceAnalyzer AudioSourceAnalyzer { get; set; }

        public Terminal<List<byte>> AudioOutputTerminal { get; set; } 

        public string SelectedDevice { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 80);
            setup.NodeName("AudioSource");
            AudioOutputTerminal = setup.OutputTerminal<List<byte>>("Audio Data", Direction.South);

            AudioSourceAnalyzer = new AudioSourceAnalyzer();
            AudioSourceAnalyzer.Lines = 32;
            AudioSourceAnalyzer.AudioDataReceived += AudioSourceAnalyzerOnAudioDataReceived;
            foreach (var deviceString in AudioSourceAnalyzer.GetDeviceList())
                Sources.Add(deviceString);
        }

        private void AudioSourceAnalyzerOnAudioDataReceived(List<byte> bytes)
        {
            AudioOutputTerminal.Data = null;
            AudioOutputTerminal.Data = bytes;
        }

        public void Enable()
        {
            AudioSourceAnalyzer.Enable(SelectedDevice);
        }
    }
}