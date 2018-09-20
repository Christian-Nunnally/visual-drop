using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Net.Sockets;

namespace DiiagramrFadeCandy
{
    /// </summary>
    public class LedChannelDriver
    {
        private const int headerLength = 4;

        public RawBox Box
        {
            get => _box;
            set
            {
                _box = value;
                _boxArea = _box.Width * _box.Height;
                _intBuffer = new int[_boxArea];
                _intermediateByteBuffer = new byte[_intBuffer.Length * sizeof(int)];
                _messageByteBuffer = new byte[headerLength + _intBuffer.Length * sizeof(byte) * 3];
            }
        }

        private int[] _intBuffer = new int[0];
        private byte[] _intermediateByteBuffer = new byte[0];
        private byte[] _messageByteBuffer = new byte[0];
        private RawBox _box = new RawBox();
        private int _boxArea;
        private byte _channel;

        public void UpdateLeds(Bitmap bitmap, Socket socket)
        {
            const int headerLength = 4;

            bitmap.CopyPixels(Box, _intBuffer);
            Buffer.BlockCopy(_intBuffer, 0, _intermediateByteBuffer, 0, _intermediateByteBuffer.Length);
            byte len_hi_byte = (byte)(_boxArea * 3 / 256);
            byte len_low_byte = (byte)(_boxArea * 3 % 256);
            _messageByteBuffer[0] = _channel;
            _messageByteBuffer[1] = 0;
            _messageByteBuffer[2] = len_hi_byte;
            _messageByteBuffer[3] = len_low_byte;

            var j = headerLength;
            for (int i = 0; i < _intermediateByteBuffer.Length; i++)
            {
                if (i % 4 == 0) continue;
                _messageByteBuffer[j++] = _intermediateByteBuffer[i];
            }

            socket.Send(_messageByteBuffer);
        }
    }
}
