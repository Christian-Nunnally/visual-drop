using System.Collections.Generic;

namespace ColorOrgan5Nodes.NodeTools
{
    public delegate void AudioDataReceivedEventHandler(List<byte> data);

    public interface IAudioSourceAnalyzer
    {
        event AudioDataReceivedEventHandler AudioDataReceived;

        int Lines { get; set; }

        IList<string> GetDeviceList();

        void Enable(string deviceString);

        void Disable();
    }
}
