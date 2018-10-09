using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;

namespace DiiagramrFadeCandy
{
    public class LedChannelDriver
    {
        public string Name { get; set; }

        public int X
        {
            get => Box.X;
            set => Box = new RawBox(value, Box.Y, Box.Width, Box.Height);
        }
        public string XText
        {
            get => X.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    X = result;
                }
            }
        }

        public int Y
        {
            get => Box.Y;
            set => Box = new RawBox(Box.X, value, Box.Width, Box.Height);
        }
        public string YText
        {
            get => Y.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    Y = result;
                }
            }
        }

        public int Width
        {
            get => Box.Width;
            set => Box = new RawBox(Box.X, Box.Y, value, Box.Height);
        }
        public string WidthText
        {
            get => Width.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    Width = result;
                }
            }
        }

        public int Height
        {
            get => Box.Height;
            set => Box = new RawBox(Box.X, Box.Y, Box.Width, value);
        }
        public string HeightText
        {
            get => Height.ToString();
            set
            {
                if (int.TryParse(value, out int result))
                {
                    Height = result;
                }
            }
        }

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
            //if (bitmap.Size.Width < Box.X + Box.Width)
            //{
            //    return _messageByteBuffer;
            //}

            //if (bitmap.Size.Height < Box.Y + Box.Height)
            //{
            //    return _messageByteBuffer;
            //}

            //if (Box.X < 0)
            //{
            //    return _messageByteBuffer;
            //}

            //if (Box.Y < 0)
            //{
            //    return _messageByteBuffer;
            //}

            if (_intBuffer.Length <= 0)
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
