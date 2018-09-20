﻿using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DiiagramrFadeCandy
{
    public class RgbTupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
    {
        public void Add(T1 item, T2 item2, T3 item3)
        {
            Add(new Tuple<T1, T2, T3>(item, item2, item3));
        }
    }

    public class FadeCandyClient : IDisposable
    {

        public bool _verbose;

        public bool _long_connection;

        public string _ip;

        public int _port;

        public Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public FadeCandyClient(string ip, int port, bool long_connecton = true, bool verbose = false)
        {
            _ip = ip;
            _port = port;
            _long_connection = long_connecton;
            _verbose = verbose;

            Debug(string.Format("{0}:{1}", _ip, _port));
        }

        private static void Debug(string message)
        {
            Console.WriteLine(message);
        }

        private bool ensureConnected()
        {
            if (_socket.Connected)
            {
                Debug("Ensure Connected: already connected, doing nothing");
                return true;
            }
            else
            {
                try
                {
                    Debug("Ensure Connected: trying to connect...");
                    _socket.Ttl = 1;
                    IPAddress ip = IPAddress.Parse(_ip);
                    _socket.Connect(ip, _port);
                    Debug("Ensure Connected: ....success");
                    return true;
                }

                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Debug("Disconnecting");
            if (_socket.Connected)
            {
                _socket.Dispose();
            }
        }

        private bool CanConnect()
        {
            bool success = ensureConnected();
            if (!_long_connection)
            {
                Dispose();
            }
            return success;
        }

        public void PutPixels(Bitmap bitmap, LedChannelDriver[] drivers)
        {
            Debug("put pixels: connecting");
            bool is_connected = ensureConnected();
            if (!is_connected)
            {
                Debug("Put pixels not connected. Ignoring these pixels.");
            }

            foreach (var driver in drivers)
            {
                if (driver == null) continue;
                driver.UpdateLeds(bitmap, _socket);
            }
        }

        internal void PutPixels(Bitmap bitmap, object ledDrivers)
        {
            throw new NotImplementedException();
        }
    }
}
