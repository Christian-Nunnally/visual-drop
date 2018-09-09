using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ColorOrganViewModel : PluginNode
    {
        private const int NumberOfPorts = 8;
        private readonly int[] _ledsAttachedToPort = new int[NumberOfPorts];
        private readonly Terminal<DisplayInfo>[] _port = new Terminal<DisplayInfo>[NumberOfPorts];
        private Timer _refreshTimer;

        private bool _clonedDisplaysNeedUpdating = false;

        private Terminal<byte[]> SerialOutputTerminal { get; set; }

        public int Port1ButtonNumber => _portButtonNumbers[0];
        public int Port2ButtonNumber => _portButtonNumbers[1];
        public int Port3ButtonNumber => _portButtonNumbers[2];
        public int Port4ButtonNumber => _portButtonNumbers[3];
        public int Port5ButtonNumber => _portButtonNumbers[4];
        public int Port6ButtonNumber => _portButtonNumbers[5];
        public int Port7ButtonNumber => _portButtonNumbers[6];
        public int Port8ButtonNumber => _portButtonNumbers[7];

        private int[] _portButtonNumbers = {1, 2, 3, 4, 5, 6, 7, 8};
        

        public void PortButtonClicked(string buttonNumber)
        {
            int buttonNumberInt = int.Parse(buttonNumber);
            _portButtonNumbers[buttonNumberInt] = (_portButtonNumbers[buttonNumberInt] + 1) % 8;
            OnPropertyChanged(nameof(Port1ButtonNumber));
            OnPropertyChanged(nameof(Port2ButtonNumber));
            OnPropertyChanged(nameof(Port3ButtonNumber));
            OnPropertyChanged(nameof(Port4ButtonNumber));
            OnPropertyChanged(nameof(Port5ButtonNumber));
            OnPropertyChanged(nameof(Port6ButtonNumber));
            OnPropertyChanged(nameof(Port7ButtonNumber));
            OnPropertyChanged(nameof(Port8ButtonNumber));

            _clonedDisplaysNeedUpdating = true;
            _skipDisplaysBecauseTheyAreCloned.Clear();
        }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 120);
            setup.NodeName("ColorOrgan");
            for (var i = 0; i < NumberOfPorts; i++)
                _port[i] = setup.InputTerminal<DisplayInfo>($"Port {i}", i >= NumberOfPorts / 2 ? Direction.East : Direction.West);

            SerialOutputTerminal = setup.OutputTerminal<byte[]>("Serial Out", Direction.South);

            _refreshTimer = new Timer(s => ClockTick(), null, 1000, 30);
        }

        private List<int> _skipDisplaysBecauseTheyAreCloned = new List<int>();
        private void ClockTick()
        {
            for (byte i = 0; i < NumberOfPorts; i++)
            {
                if (_skipDisplaysBecauseTheyAreCloned.Contains(i)) continue;
                var displayInfo = _port[i].Data;
                if (displayInfo == null) continue;
                var header = new byte[] {40, 30, 20, 1, i};
                IEnumerable<byte> outputBytes = new byte[0];
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

                if (_clonedDisplaysNeedUpdating)
                {
                    SerialOutputTerminal.Data = new byte[] { 40, 30, 20, 3, i, 0 };

                    var displayNumber = _portButtonNumbers[i];
                    for (byte j = 0; j < NumberOfPorts; j++)
                    {
                        if (i == j) continue;
                        if (_portButtonNumbers[j] == displayNumber)
                        {
                            SerialOutputTerminal.Data = new byte[] { 40, 30, 20, 2, i, j };
                            _skipDisplaysBecauseTheyAreCloned.Add(j);
                        }
                    }

                    _clonedDisplaysNeedUpdating = false;
                }

                var outputByteArray = outputBytes as byte[] ?? outputBytes.ToArray();
                if (outputByteArray.Length == 3 * totalLedCount && totalLedCount > 0)
                    SerialOutputTerminal.Data = header.Concat(outputByteArray).ToArray();
            }
        }
    }
}