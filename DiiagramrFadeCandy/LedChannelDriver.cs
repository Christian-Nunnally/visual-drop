using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class LedChannelDriver
    {
        [DataMember]
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public event Action<int> UpdateFrame;

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        [DataMember]
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

        private const int NumberOfLeds = 64;
        private int[] _intBuffer = new int[0];
        private byte[] _intermediateByteBuffer = new byte[0];
        private readonly byte[] _messageByteBuffer = new byte[NumberOfLeds * 3];
        private RawBox _box = new RawBox();
        private int _boxArea;

        [NonSerialized]
        private Bitmap _attachedBitmap;

        public byte[] GetLedData(int frameNumber)
        {
            UpdateFrame?.Invoke(frameNumber);
            if (AttachedBitmap == null)
            {
                return _messageByteBuffer;
            }

            if (AttachedBitmap.Size.Width < Box.X + Box.Width)
            {
                return _messageByteBuffer;
            }

            if (AttachedBitmap.Size.Height < Box.Y + Box.Height)
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

            if (_intBuffer.Length <= 0)
            {
                return _messageByteBuffer;
            }

            if (Box.Y * Box.X > 64)
            {
                return _messageByteBuffer;
            }

            AttachedBitmap.CopyPixels(Box, _intBuffer);
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

        public Bitmap AttachedBitmap { get => _attachedBitmap; set => _attachedBitmap = value; }
    }
}
