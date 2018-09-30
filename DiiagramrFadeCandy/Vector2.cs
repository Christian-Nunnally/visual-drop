using SharpDX.Mathematics.Interop;
using System;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
        public RawVector2 RawVector => new RawVector2(X, Y);

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            const string format = "0.00";
            return $"({X.ToString(format)}, {X.ToString(format)})";
        }
    }
}
