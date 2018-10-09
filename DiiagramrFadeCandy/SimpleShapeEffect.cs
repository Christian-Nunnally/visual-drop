using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    public enum Shape
    {
        Rectangle,
        Ellipse,
        RoundedRectangle
    }

    [Serializable]
    public class ShapeEffect : IGraphicEffect
    {

        [DataMember]
        public Shape Mode { get; set; } = Shape.Ellipse;

        [DataMember]
        public bool Visible { get; set; }

        [DataMember]
        public float X { get; set; }

        [DataMember]
        public float Y { get; set; }

        [DataMember]
        public float Width { get; set; }

        [DataMember]
        public float Height { get; set; }

        [DataMember]
        public float R { get; set; } = 0.1f;

        [DataMember]
        public float G { get; set; } = 0.1f;

        [DataMember]
        public float B { get; set; } = 0.1f;

        [DataMember]
        public float A { get; set; } = 0.1f;

        [DataMember]
        public bool Fill { get; set; } = true;

        public void Draw(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            var brush = new SolidColorBrush(target, new RawColor4(R, G, B, A));

            switch (Mode)
            {
                case Shape.Rectangle:
                    var rectangle = new RawRectangleF(X, Y, X + Width, Y + Height);
                    if (Fill)
                    {
                        target.FillRectangle(rectangle, brush);
                    }
                    else
                    {
                        target.DrawRectangle(rectangle, brush);
                    }
                    break;
                case Shape.Ellipse:
                    var radiusX = Width / 2.0f;
                    var radiusY = Height / 2.0f;
                    var ellipse = new Ellipse(new RawVector2(X + radiusX, Y + radiusY), radiusX, radiusY);
                    if (Fill)
                    {
                        target.FillEllipse(ellipse, brush);
                    }
                    else
                    {
                        target.DrawEllipse(ellipse, brush);
                    }
                    break;
                case Shape.RoundedRectangle:
                    radiusX = Width / 4.0f;
                    radiusY = Height / 4.0f;
                    if (Fill)
                    {
                        rectangle = new RawRectangleF(X, Y, X + Width, Y + Height);
                        var roundedRectangle = new RoundedRectangle { Rect = rectangle, RadiusX = radiusX, RadiusY = radiusY };
                        target.FillRoundedRectangle(roundedRectangle, brush);
                    }
                    else
                    {
                        rectangle = new RawRectangleF(X, Y, X + Width, Y + Height);
                        var roundedRectangle = new RoundedRectangle { Rect = rectangle, RadiusX = radiusX, RadiusY = radiusY };
                        target.DrawRoundedRectangle(roundedRectangle, brush);
                    }
                    break;
            }
        }
    }
}
