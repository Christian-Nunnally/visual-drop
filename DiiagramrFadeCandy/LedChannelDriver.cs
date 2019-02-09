using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class LedChannelDriver
    {
        // The FadeCandy limits the number of leds per channel to 64.
        private const int MaxChannelSize = 64;

        [DataMember]
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public bool AlternateStrideDirection { get; set; }

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
                _intBuffer = new int[Math.Min(_boxArea, MaxChannelSize)];
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

            // Outside of the frame.
            if (AttachedBitmap.Size.Width < Box.X || AttachedBitmap.Size.Height < Box.Y || Box.X + Box.Width < 0 || Box.Y + Box.Height < 0)
            {
                return _messageByteBuffer;
            }

            if (Box.X < 0)
            {
                X = 0;
            }

            if (Box.Y < 0)
            {
                Y = 0;
            }

            if (AttachedBitmap.Size.Width < Box.X + Box.Width)
            {
                Width = AttachedBitmap.Size.Width - Box.X;
            }

            if (AttachedBitmap.Size.Height < Box.Y + Box.Height)
            {
                Height = AttachedBitmap.Size.Height - Box.Y;
            }

            if (_intBuffer.Length <= 0 || _intBuffer.Length > MaxChannelSize)
            {
                return _messageByteBuffer;
            }

            if (Box.Width * Box.Height > NumberOfLeds)
            {
                return _messageByteBuffer;
            }

            AttachedBitmap.CopyPixels(Box, _intBuffer);
            Buffer.BlockCopy(_intBuffer, 0, _intermediateByteBuffer, 0, _intermediateByteBuffer.Length);
            var j = 0;
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    var pixelIndex = row * Width + col;
                    var invertedRowPixelIndex = (row * Width) + (Width - col - 1);
                    var oddRow = row % 2 == 0;
                    var copyFromPixelIndex = AlternateStrideDirection
                        ? oddRow ? pixelIndex : invertedRowPixelIndex
                        : pixelIndex;
                    CopyIntermediateBufferToMessageBuffer(pixelIndex, copyFromPixelIndex);
                }
            }

            return _messageByteBuffer;
        }

        private void CopyIntermediateBufferToMessageBuffer(int toPixelIndex, int fromPixelIndex)
        {
            var toPixelColorIndex = toPixelIndex * 3;
            var fromPixelColorIndex = fromPixelIndex * 4;
            _messageByteBuffer[toPixelColorIndex + 0] = _intermediateByteBuffer[fromPixelColorIndex + 2];
            _messageByteBuffer[toPixelColorIndex + 1] = _intermediateByteBuffer[fromPixelColorIndex + 1];
            _messageByteBuffer[toPixelColorIndex + 2] = _intermediateByteBuffer[fromPixelColorIndex + 0];
        }

        public Bitmap AttachedBitmap { get => _attachedBitmap; set => _attachedBitmap = value; }
    }
}
