using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class Color : ISerializable
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public RawColor4 RawColor => new RawColor4(R, G, B, A);

        public Color(SerializationInfo info, StreamingContext context)
        {
            R = (float)info.GetValue(nameof(R), typeof(float));
            G = (float)info.GetValue(nameof(G), typeof(float));
            B = (float)info.GetValue(nameof(B), typeof(float));
            A = (float)info.GetValue(nameof(A), typeof(float));
        }

        public Color(float r, float g, float b, float a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public override string ToString()
        {
            const string format = "0.00";
            return $"({R.ToString(format)}, {G.ToString(format)}, {B.ToString(format)}, {A.ToString(format)})";
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(R), R);
            info.AddValue(nameof(G), G);
            info.AddValue(nameof(B), B);
            info.AddValue(nameof(A), A);
        }
    }
}
