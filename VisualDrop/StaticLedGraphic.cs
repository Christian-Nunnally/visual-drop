using System;
using System.Text;
using System.Windows.Media;

namespace VisualDrop
{
    public class StaticLedGraphic
    {
        private readonly int _numLeds;

        public StaticLedGraphic(int numLeds)
        {
            _numLeds = numLeds;
            Graphic = new byte[numLeds * 3];
        }
        
        public byte[] Graphic { get; }

        public void SetPixel(int pixelNumber, Color color)
        {
            if (pixelNumber >= _numLeds) return;
            Graphic[pixelNumber * 3] = color.R;
            Graphic[pixelNumber * 3 + 1] = color.G;
            Graphic[pixelNumber * 3 + 2] = color.B;
        }

        public bool IsPixelOff(int pixelNumber)
        {
            if ((pixelNumber < 0) || (pixelNumber >= 64)) throw new ArgumentException("color");
            return (Graphic[pixelNumber * 3] == 0) && (Graphic[pixelNumber * 3 + 1] == 0) && (Graphic[pixelNumber * 3 + 2] == 0);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var b in Graphic)
            {
                sb.Append(b);
                sb.Append("-");
            }
            return sb.ToString();
        }
    }
}