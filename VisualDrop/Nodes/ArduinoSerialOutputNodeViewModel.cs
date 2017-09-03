using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using DiagramEditor.Model;
using DiagramEditor.Service;
using DiagramEditor.ViewModel.Diagram;
using Stylet;

namespace ColorOrgan5Nodes.Nodes
{
    public class ArduinoSerialOutputNodeViewModel : PluginNodeViewModel
    {
        private const string SerialConnectedStatusString = "Connected!";
        private const string SerialDisconnectedStatusString = "Connect";
        private const string GraphicsDataIn1DelegateKey = "GraphicsDataIn1";
        private const string GraphicsDataIn2DelegateKey = "GraphicsDataIn2";
        private const string GraphicsDataIn3DelegateKey = "GraphicsDataIn3";
        private const string GraphicsDataIn4DelegateKey = "GraphicsDataIn4";
        private static bool _isSerialThreadRunning;
        private readonly ConcurrentQueue<string> _inputQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<byte[]> _ouputQueue = new ConcurrentQueue<byte[]>();
        private bool _keepRunningSerialThread;

        public override string Name => "Arduino Serial Output";

        private SerialPort Serial { get; set; }

        public BindableCollection<string> PortNames { get; set; }

        public string SerialButtonContent { get; set; }

        public string SelectedPort { get; set; }

        public bool ConnectButtonIsEnabled { get; set; }

        public override void NodeLoaded()
        {
            PortNames = new BindableCollection<string>();
            StartWatchingForUsbEvents();
            SerialButtonContent = SerialDisconnectedStatusString;
            RefreshUsbPorts();
        }

        public override void ConstructTerminals()
        {
            ConstructNewInputTerminal("Image #1", typeof(byte[]), Direction.North, GraphicsDataIn1DelegateKey);
            ConstructNewInputTerminal("Image #2", typeof(byte[]), Direction.North, GraphicsDataIn2DelegateKey);
            ConstructNewInputTerminal("Image #3", typeof(byte[]), Direction.North, GraphicsDataIn3DelegateKey);
            ConstructNewInputTerminal("Image #4", typeof(byte[]), Direction.North, GraphicsDataIn4DelegateKey);
        }

        public override void SetupDelegates(DelegateMapper delegateMapper)
        {
            delegateMapper.AddMapping(GraphicsDataIn1DelegateKey, o => GraphicsDataIn(o, 0));
            delegateMapper.AddMapping(GraphicsDataIn2DelegateKey, o => GraphicsDataIn(o, 1));
            delegateMapper.AddMapping(GraphicsDataIn3DelegateKey, o => GraphicsDataIn(o, 2));
            delegateMapper.AddMapping(GraphicsDataIn4DelegateKey, o => GraphicsDataIn(o, 3));
        }

        private void StartWatchingForUsbEvents()
        {
            StartWatchingWithWmi("Win32_DeviceChangeEvent", 2);
            StartWatchingWithWmi("Win32_DeviceChangeEvent", 3);
        }

        private void StartWatchingWithWmi(string eventName, int eventType)
        {
            var query = new WqlEventQuery($"SELECT * FROM {eventName} WHERE EventType = {eventType}");
            var watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += UsbEvent;
            watcher.Start();
        }

        private void UsbEvent(object sender, EventArrivedEventArgs e)
        {
            RefreshUsbPorts();
        }

        private void RefreshUsbPorts()
        {
            var identifiedPorts = SerialPort.GetPortNames();
            foreach (var identifiedPort in identifiedPorts) if (!PortNames.Contains(identifiedPort)) PortNames.Add(identifiedPort);
            for (var i = PortNames.Count - 1; i >= 0; i--) if (!identifiedPorts.Contains(PortNames[i])) PortNames.RemoveAt(i);
            if (string.IsNullOrWhiteSpace(SelectedPort)) SelectedPort = PortNames.FirstOrDefault();
        }

        private IDictionary<OutputTerminal, object> GraphicsDataIn(object data, byte displayNumber)
        {
            var graphicsData = (IList<byte>) data;
            if (!_isSerialThreadRunning) return null;
            if (graphicsData.Count < 64*3) return null;

            var header = CreateHeaderForDisplay(displayNumber);
            _ouputQueue.Enqueue(header.Concat(graphicsData).ToArray());
            return null;
        }

        private static IEnumerable<byte> CreateHeaderForDisplay(byte displayIndex)
        {
            return new byte[] {40, 30, 20, 10, displayIndex, 0, 0, 0, 0, 0, 0, 0};
        }

        private void RunArduinoSerialInterfaceBuffer()
        {
            SerialButtonContent = "Connecting...";
            ConnectButtonIsEnabled = false;
            if (_keepRunningSerialThread) throw new InvalidOperationException("You have to wait for one serial thread to end before starting another one.");
            SerialPort serial;
            _keepRunningSerialThread = true;

            if (!TryOpenSerialConnection(SelectedPort, out serial))
            {
                _keepRunningSerialThread = false;
                SerialButtonContent = "Connection failed";
                ConnectButtonIsEnabled = true;
                return;
            }
            Serial = serial;
            ConnectButtonIsEnabled = true;

            _isSerialThreadRunning = true;

            SerialButtonContent = SerialConnectedStatusString;

            while (_keepRunningSerialThread && serial.IsOpen)
                TryToSendFromOutputQueue();
            CloseSerialConnection(serial);

            SerialButtonContent = SerialDisconnectedStatusString;

            _isSerialThreadRunning = false;
            SerialButtonContent = SerialDisconnectedStatusString;
        }

        private bool TryOpenSerialConnection(string portName, out SerialPort openPort)
        {
            openPort = null;
            if (string.IsNullOrWhiteSpace(portName)) return false;

            var serial = new SerialPort(SelectedPort);
            serial.BaudRate = 115200;
            serial.StopBits = StopBits.One;
            serial.Parity = Parity.None;
            serial.DataBits = 8;
            serial.DtrEnable = true;

            try
            {
                serial.Open();
            }
            catch (Exception)
            {
                return false;
            }

            openPort = serial;
            return true;
        }

        private void CloseSerialConnection(SerialPort port)
        {
            port.Close();
            port.Dispose();
        }

        private void TryToSendFromOutputQueue()
        {
            byte[] wasDequeued;
            if (_ouputQueue.TryDequeue(out wasDequeued))
                try
                {
                    Serial.Write(wasDequeued, 0, wasDequeued.Length);
                }
                catch (Exception)
                {
                    throw;
                }
        }

        private void WaitToReceiveZeroAndEnqueueOtherMessages(ref bool cancel)
        {
            while (cancel)
            {
                var serialResponse = WaitForSerialResponse();
                if ((serialResponse.Length == 1) && (serialResponse[0] == 0)) return;
                serialResponse.ForEach(b => _inputQueue.Enqueue(b.ToString()));
            }
        }

        private byte[] WaitForSerialResponse()
        {
            while (Serial.BytesToRead == 0) Thread.Sleep(1);
            var serialResponse = new byte[Serial.BytesToRead];
            Serial.Read(serialResponse, 0, serialResponse.Length);
            return serialResponse;
        }

        private void DisableSerialInterface()
        {
            _keepRunningSerialThread = false;
            SerialButtonContent = "Disconnecting...";
            ConnectButtonIsEnabled = false;
            while (_isSerialThreadRunning)
            {
            }
            ConnectButtonIsEnabled = true;
        }

        private void EnableSerialInterface()
        {
            ThreadStart threadStart = RunArduinoSerialInterfaceBuffer;
            var arduinoInterfaceBufferThread = new Thread(threadStart);
            arduinoInterfaceBufferThread.Start();
        }

        public void SerialButtonPressed()
        {
            if (_isSerialThreadRunning) DisableSerialInterface();
            else EnableSerialInterface();
            SerialButtonContent = SerialDisconnectedStatusString;
        }
    }
}