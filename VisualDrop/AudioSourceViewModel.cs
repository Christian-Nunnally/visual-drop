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
        private int _lines;
        public ObservableCollection<string> Sources { get; set; } = new ObservableCollection<string>();
        public AudioSourceAnalyzer AudioSourceAnalyzer { get; set; }

        public Terminal<byte[]> AudioOutputTerminal { get; set; } 

        public string SelectedDevice { get; set; }

        public int AverageDataReceivedPerSecond { get; set; }

        public bool Enabled { get; set; }
        public string ToggleEnableButtonText => Enabled ? "Enabled" : "Enable";

        private Stopwatch _lastTimeAudioDataReceivedStopwatch = new Stopwatch();
        private long _averageTimeBetweenAudioData;

        public SolidColorBrush EnabledIndicatorColor => Enabled ? Brushes.LightGreen : Brushes.LightCoral;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(200, 80);
            setup.NodeName("AudioSource");
            AudioOutputTerminal = setup.OutputTerminal<byte[]>("Audio Data", Direction.South);

            AudioSourceAnalyzer = new AudioSourceAnalyzer();
            Lines = 64;
            AudioSourceAnalyzer.AudioDataReceived += AudioSourceAnalyzerOnAudioDataReceived;
            foreach (var deviceString in AudioSourceAnalyzer.GetDeviceList())
                Sources.Add(deviceString);
            SelectedDevice = Sources.LastOrDefault();
        }

        public int Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                AudioSourceAnalyzer.Lines = _lines;
            }
        }

        private void AudioSourceAnalyzerOnAudioDataReceived(List<byte> bytes)
        {
            var msSinceLastCall = _lastTimeAudioDataReceivedStopwatch.ElapsedMilliseconds;
            _averageTimeBetweenAudioData = (_averageTimeBetweenAudioData * 5 + msSinceLastCall) / 6;
            AverageDataReceivedPerSecond = (int) (1000 / (_averageTimeBetweenAudioData == 0 ? 1 : _averageTimeBetweenAudioData));
            _lastTimeAudioDataReceivedStopwatch.Restart();
            AudioOutputTerminal.Data = null;
            AudioOutputTerminal.Data = bytes.ToArray();
        }

        public void ToggleEnable()
        {
            if (Enabled) AudioSourceAnalyzer.Disable();
            else AudioSourceAnalyzer.Enable(SelectedDevice);
            Enabled = !Enabled;
        }

        public void AddLine()
        {

            Lines++;
        }

        public void RemoveLine()
        {
            if (Lines > 0) Lines--;
        }
    }
}