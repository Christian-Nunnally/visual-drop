using System;
using System.Text;
using System.Windows.Media;

namespace ColorOrgan5Nodes.NodeTools
{
    public class StaticLedGraphic
    {
        public StaticLedGraphic()
        {
            Graphic = new byte[64 * 3];
        }
        
        public byte[] Graphic { get; }

        public void SetPixel(int pixelNumber, Color color)
        {
            if ((pixelNumber < 0) || (pixelNumber >= 64)) throw new ArgumentException("color");
            Graphic[pixelNumber] = color.R;
            Graphic[pixelNumber + 64] = color.G;
            Graphic[pixelNumber + 128] = color.B;
        }

        public bool IsPixelOff(int pixelNumber)
        {
            if ((pixelNumber < 0) || (pixelNumber >= 64)) throw new ArgumentException("color");
            return (Graphic[pixelNumber] == 0) && (Graphic[pixelNumber + 64] == 0) && (Graphic[pixelNumber + 64] == 0);
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