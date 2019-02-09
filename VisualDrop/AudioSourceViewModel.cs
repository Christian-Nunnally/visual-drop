using DiiagramrAPI.PluginNodeApi;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace VisualDrop
{
    public class AudioSourceViewModel : PluginNode
    {
        private const int DefaultLines = 4;
        private int _lines;
        public ObservableCollection<AudioDeviceInformation> Sources { get; set; } = new ObservableCollection<AudioDeviceInformation>();
        public AudioSourceAnalyzer AudioSourceAnalyzer { get; set; }

        public Terminal<byte[]> AudioOutputTerminal { get; set; }
        public Terminal<int> LinesInputTerminal { get; private set; }

        [PluginNodeSetting]
        public AudioDeviceInformation SelectedDevice
        {
            get => _selectedDevice;

            set
            {
                _selectedDevice = value;
                Properties.Settings.Default.LastAudioDevice = _selectedDevice.Name;
                Properties.Settings.Default.Save();
            }
        }

        public int AverageDataReceivedPerSecond { get; set; }

        public bool Enabled { get; set; }
        public bool NotEnabled => !Enabled;
        public string ToggleEnableButtonText => Enabled ? "Enabled" : "Enable";

        private Stopwatch _lastTimeAudioDataReceivedStopwatch = new Stopwatch();
        private long _averageTimeBetweenAudioData;
        private AudioDeviceInformation _selectedDevice;

        public SolidColorBrush EnabledIndicatorColor => Enabled ? Brushes.LightGreen : Brushes.LightCoral;

        private BackgroundWorker GetUpdatedDeviceListWorker { get; set; } = new BackgroundWorker();

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(120, 60);
            setup.NodeName("AudioSource");
            setup.NodeWeight(0);
            AudioOutputTerminal = setup.OutputTerminal<byte[]>("Audio Data", Direction.South);
            LinesInputTerminal = setup.InputTerminal<int>("Buckets", Direction.West);
            LinesInputTerminal.DataChanged += LinesInputTerminal_DataChanged;

            AudioSourceAnalyzer = AudioSourceAnalyzer.Instance;
            GetUpdatedDeviceListWorker.DoWork += LoadSources;
            GetUpdatedDeviceListWorker.RunWorkerCompleted += SourcesLoaded;

            Lines = DefaultLines;
            AudioSourceAnalyzer.AudioDataReceived += AudioSourceAnalyzerOnAudioDataReceived;

            GetUpdatedDeviceListWorker.RunWorkerAsync();
        }

        private void LinesInputTerminal_DataChanged(int lines)
        {
            if (lines <= 0)
            {
                Lines = DefaultLines;
                return;
            }
            Lines = lines;
        }

        private void SourcesLoaded(object sender, RunWorkerCompletedEventArgs e)
        {
            var sources = (IList<string>)e.Result;
            foreach (var source in sources)
            {
                if (!Sources.Any(x => x.Name == source))
                {
                    Sources.Add(new AudioDeviceInformation
                    {
                        Name = source
                    });
                }
            }

            if (!string.IsNullOrEmpty(SelectedDevice?.Name) && sources.Contains(SelectedDevice.Name))
            {
                return;
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastAudioDevice) && sources.Contains(Properties.Settings.Default.LastAudioDevice))
            {
                SelectedDevice = Sources.First(x => x.Name == Properties.Settings.Default.LastAudioDevice);
                return;
            }

            SelectedDevice = Sources.LastOrDefault();
        }

        private void LoadSources(object sender, DoWorkEventArgs e)
        {
            e.Result = AudioSourceAnalyzer.GetDeviceList();
        }

        public int Lines
        {
            get => _lines;

            set
            {
                _lines = value;
                if (AudioSourceAnalyzer != null)
                {
                    AudioSourceAnalyzer.Lines = _lines;
                }
            }
        }

        private void AudioSourceAnalyzerOnAudioDataReceived(List<byte> bytes)
        {
            var msSinceLastCall = _lastTimeAudioDataReceivedStopwatch.ElapsedMilliseconds;
            _averageTimeBetweenAudioData = (_averageTimeBetweenAudioData * 5 + msSinceLastCall) / 6;
            AverageDataReceivedPerSecond = (int)(1000 / (_averageTimeBetweenAudioData == 0 ? 1 : _averageTimeBetweenAudioData));
            _lastTimeAudioDataReceivedStopwatch.Restart();
            AudioOutputTerminal.Data = null;
            AudioOutputTerminal.Data = bytes.ToArray();
        }

        public void ToggleEnable()
        {
            if (Enabled)
            {
                AudioSourceAnalyzer.Disable();
            }
            else
            {
                AudioSourceAnalyzer.Enable(SelectedDevice?.Name);
            }

            Enabled = AudioSourceAnalyzer.IsEnabled;
        }

        public void AddLine()
        {
            Lines++;
        }

        public void RemoveLine()
        {
            if (Lines > 0)
            {
                Lines--;
            }
        }
    }
}
