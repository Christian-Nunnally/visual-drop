using SharpDX.Mathematics.Interop;
using System;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class Color
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }
        public RawColor4 RawColor => new RawColor4(R, G, B, A);

        public Color(float r, float g, float b, float a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString()
        {
            const string format = "0.00";
            return $"({R.ToString(format)}, {G.ToString(format)}, {B.ToString(format)}, {A.ToString(format)})";
        }
    }
}
