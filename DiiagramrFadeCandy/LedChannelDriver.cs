using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;

namespace DiiagramrFadeCandy
{
    public class LedChannelDriver
    {
        public RawBox Box
        {
            get => _box;
            set
            {
                _box = value;
                _boxArea = _box.Width * _box.Height;
                _intBuffer = new int[_boxArea];
                _intermediateByteBuffer = new byte[_intBuffer.Length * sizeof(int)];
            }
        }

        private int[] _intBuffer = new int[0];
        private byte[] _intermediateByteBuffer = new byte[0];
        private readonly byte[] _messageByteBuffer = new byte[64 * 3];
        private RawBox _box = new RawBox();
        private int _boxArea;

        public byte[] GetLedData(Bitmap bitmap)
        {
            if (bitmap.Size.Width < Box.X + Box.Width)
            {
                return _messageByteBuffer;
            }

            if (bitmap.Size.Height < Box.Y + Box.Height)
            {
                return _messageByteBuffer;
            }

            if (Box.X < 0)
            {
                return _messageByteBuffer;
            }

            if (Box.Y < 0)
            {
                return _messageByteBuffer;
            }

            bitmap.CopyPixels(Box, _intBuffer);
            Buffer.BlockCopy(_intBuffer, 0, _intermediateByteBuffer, 0, _intermediateByteBuffer.Length);
            var j = 0;
            for (int i = 0; i < _intermediateByteBuffer.Length; i += 4)
            {
                _messageByteBuffer[j++] = _intermediateByteBuffer[i + 2];
                _messageByteBuffer[j++] = _intermediateByteBuffer[i + 1];
                _messageByteBuffer[j++] = _intermediateByteBuffer[i];
            }

            return _messageByteBuffer;
        }
    }
}
