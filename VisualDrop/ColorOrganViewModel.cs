using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ColorOrganViewModel : PluginNode
    {
        private const int NumberOfPorts = 8;
        private readonly int[] _ledsAttachedToPort = new int[NumberOfPorts];
        private readonly Terminal<DisplayInfo>[] _port = new Terminal<DisplayInfo>[NumberOfPorts];

        private Terminal<byte[]> SerialOutputTerminal { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 120);
            setup.NodeName("ColorOrgan");
            for (var i = 0; i < NumberOfPorts; i++)
                _port[i] = setup.InputTerminal<DisplayInfo>($"Port {i}", i >= NumberOfPorts / 2 ? Direction.East : Direction.West);

            SerialOutputTerminal = setup.OutputTerminal<byte[]>("Serial Out", Direction.South);

            new Thread(ClockTick).Start();
        }

        private void ClockTick()
        {
            while (true)
            {
                Thread.Sleep(17);
                for (byte i = 0; i < NumberOfPorts; i++)
                {
                    var header = new byte[] {40, 30, 20, 1, i};
                    IEnumerable<byte> outputBytes = new byte[0];
                    var displayInfo = _port[i].Data;
                    byte totalLedCount = 0;
                    while (displayInfo != null)
                    {
                        totalLedCount += displayInfo.DisplaySize;
                        var graphic = displayInfo.VisualEffect?.GetEffect();
                        outputBytes = outputBytes.Concat(graphic ?? new byte[displayInfo.DisplaySize * 3]);
                        displayInfo = displayInfo.ChainedDisplay;
                    }

                    if (_ledsAttachedToPort[i] != totalLedCount)
                    {
                        _ledsAttachedToPort[i] = totalLedCount;
                        SerialOutputTerminal.Data = new byte[] {40, 30, 20, 0, i, totalLedCount};
                    }

                    var outputByteArray = outputBytes as byte[] ?? outputBytes.ToArray();
                    if (outputByteArray.Length == 3 * totalLedCount && totalLedCount > 0)
                        SerialOutputTerminal.Data = header.Concat(outputByteArray).ToArray();
                }
            }
        }
    }
}