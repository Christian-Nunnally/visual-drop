using ColorOrgan5Nodes.NodeTools;
using DiagramEditor.Service;
using DiagramEditor.ViewModel.Diagram;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace ColorOrgan5Nodes.Nodes
{
    public class AudioSourceNodeViewModel : PluginNodeViewModel
    {
        private IAudioSourceAnalyzer _audioAnalyzer;

        public ObservableCollection<string> DeviceList { get; set; }

        public string SelectedDevice { get; set; }

        public override string Name => "Audio Source Node";

        public override void ConstructTerminals()
        {
            ConstructNewOutputTerminal("Output", typeof(List<byte>), Direction.South);
        }

        public override void NodeLoaded()
        {
            DeviceList = new ObservableCollection<string>();

            _audioAnalyzer = new AudioSourceAnalyzer();
            _audioAnalyzer.Lines = 4;
            _audioAnalyzer.AudioDataReceived += OnAudioAnalyzerAudioDataReceived;
            UpdateDeviceList();
        }

        private void OnAudioAnalyzerAudioDataReceived(List<byte> data)
        {
            ExecuteFromOutput(data);
        }

        private void UpdateDeviceList()
        {
            var updatedDeviceList = _audioAnalyzer.GetDeviceList();

            _audioAnalyzer.GetDeviceList().ForEach(device =>
            {
                if (!DeviceList.Any(device2 => device.ToString().Equals(device2.ToString()))) DeviceList.Add(device);
            });

            DeviceList.ForEach(device =>
            {
                if (!updatedDeviceList.Any(device2 => device.ToString().Equals(device2.ToString()))) DeviceList.Remove(device);
            });
        }

        public void DeviceSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _audioAnalyzer.Disable();

            if (e.AddedItems.Count != 1) return;
            var addedItem = e.AddedItems[0] as string;
            if (string.IsNullOrWhiteSpace(addedItem)) return;
            UpdateDeviceList();

            _audioAnalyzer.Enable(addedItem);
        }
    }
}

