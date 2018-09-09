using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using DiiagramrAPI.PluginNodeApi;
using Stylet;

namespace VisualDrop
{
    public class SerialOutputViewModel : PluginNode, INotifyPropertyChanged
    {
        private const string DisconnectString = "Disconnect";
        private const string ConnectString = "Connect";
        private const string ConnectingString = "Connecting...";
        private readonly ConcurrentQueue<string> _rxQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<byte[]> _txQueue = new ConcurrentQueue<byte[]>();
        private bool _keepConnectionAlive;
        private string _sendBytesString;
        private byte[] _sendBytes;

        private Terminal<byte[]> TransmitTerminal { get; set; }
        public BindableCollection<string> PortNames { get; set; } = new BindableCollection<string>();

        public string ConnectButtonText { get; set; } = ConnectString;
        public string DisconnectButtonText { get; set; } = DisconnectString;
        public string SelectedPort { get; set; }
        public string ErrorButtonText { get; set; }
        public bool ErrorButtonVisible => ErrorButtonText != null;

        public bool IsSerialThreadRunning { get; set; }

        public int TxCount { get; set; }
        public int RxCount { get; set; }

        public string SendBytes
        {
            get => _sendBytesString;
            set
            {
                _sendBytesString = value;
                _sendBytesString = _sendBytesString.Trim().Replace(" ", "");
                var ss = _sendBytesString.Split(',');
                _sendBytes = new byte[ss.Length];
                var i = 0;
                foreach (var stringByte in ss)
                {
                    if (byte.TryParse(stringByte, out var b))
                    {
                        _sendBytes[i] = b;
                    }
                    else
                    {
                        _sendBytes = null;
                        break;
                    }
                    i++;
                }
            }
        }

        public string LastRx { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(200, 120);
            setup.NodeName("Serial");
            TransmitTerminal = setup.InputTerminal<byte[]>("Byte Array", Direction.North);
            TransmitTerminal.DataChanged += TransmitTerminalOnDataChanged;

            PortNames.AddRange(SerialPort.GetPortNames());
            SelectedPort = PortNames.LastOrDefault();
        }

        private void TransmitTerminalOnDataChanged(byte[] data)
        {
            if (data == null) return;
            if (!IsSerialThreadRunning) return;

            TransmitData(data);
        }

        private void TransmitData(byte[] data)
        {
            _txQueue.Enqueue(data);
            TxCount = _txQueue.Count;
        }

        private void RunSerialTxRxLoop()
        {
            if (_keepConnectionAlive)
            {
                ErrorButtonText = "Already connected!";
                return;
            }
            _keepConnectionAlive = true;

            try
            {
                var serialDevice = OpenSerialPort(SelectedPort);
                IsSerialThreadRunning = true;
                while (_keepConnectionAlive && serialDevice != null && serialDevice.IsOpen)
                {
                    try
                    {
                        TransmitFromQueue(serialDevice);
                        Thread.Sleep(1);

                        if (serialDevice.BytesToRead != 0)
                        {
                            WaitForAndEnqueueSerialResponse(serialDevice, 500);
                        }
                    }
                    catch (IOException e)
                    {
                        break;
                    }
                    catch (InvalidOperationException e)
                    {
                        break;
                    }
                }
                CloseSerialPort(serialDevice);
            }
            catch (Exception e)
            {
                ErrorButtonText = e.Message;
            }

            IsSerialThreadRunning = false;
            Disconnect();
        }

        private void WaitForAndEnqueueSerialResponse(SerialPort serialDevice, int timeout)
        {
            var serialResponse = WaitForSerialResponse(serialDevice, timeout);
            if (serialResponse == null) return;
            foreach (var b in serialResponse)
            {
                _rxQueue.Enqueue(b.ToString());
            }
        }

        private byte[] WaitForSerialResponse(SerialPort serialDevice, int timeout)
        {
            var t = 0;
            while (serialDevice.BytesToRead == 0)
            {
                Thread.Sleep(1);
                if (t++ > timeout) return null;
            }
            var serialResponse = new byte[serialDevice.BytesToRead];
            serialDevice.Read(serialResponse, 0, serialResponse.Length);
            return serialResponse;
        }

        private SerialPort OpenSerialPort(string portName)
        {
            if (string.IsNullOrWhiteSpace(portName)) throw new ArgumentNullException();

            var serial = new SerialPort(SelectedPort)
            {
                BaudRate = 115200,
                StopBits = StopBits.One,
                Parity = Parity.None,
                DataBits = 8,
                DtrEnable = true
            };

            try
            {
                serial.Open();
            }
            catch (IOException e)
            {
            }
            return serial;
        }

        private void CloseSerialPort(SerialPort serialDevice)
        {
            serialDevice.Close();
            serialDevice.Dispose(); 
        }

        private void TransmitFromQueue(SerialPort serialDevice)
        {
            if (!_txQueue.TryDequeue(out var wasDequeued)) return;
            try
            {
                serialDevice.Write(wasDequeued, 0, wasDequeued.Length);
                TxCount = _txQueue.Count;
            }
            catch (IOException e)
            {
                Disconnect();
            }
        }

        public void Connect()
        {
            ConnectButtonText = ConnectingString;
            new Thread(RunSerialTxRxLoop).Start();
        }

        public void Disconnect()
        {
            _keepConnectionAlive = false;
            ConnectButtonText = ConnectString;
        }

        public void ClearError()
        {
            ErrorButtonText = null;
            ConnectButtonText = ConnectString;
        }

        public void ManualSend()
        {
            TransmitData(_sendBytes);
        }

        public void RxFromQueue()
        {
            if (_rxQueue.IsEmpty) return;
            if (_rxQueue.TryDequeue(out var lastRx))
            {
                LastRx = lastRx;
            }
        }
    }
}