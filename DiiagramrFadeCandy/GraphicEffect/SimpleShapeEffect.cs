using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    [Serializable]
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
        public float Thickness { get; set; }

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
        public float A { get; set; } = 0.05f;

        [DataMember]
        public bool Fill { get; set; } = true;

        public void Draw(RenderTarget target)
        {
            if (!Visible)
            {
                return;
            }

            var brush = new SolidColorBrush(target, new RawColor4(R, G, B, A));
            var xRadius = Width * target.Size.Width / 2.0f;
            var yRadius = Height * target.Size.Height / 2.0f;
            var left = X * target.Size.Width - xRadius;
            var top = Y * target.Size.Height - yRadius;
            var right = X * target.Size.Width + xRadius;
            var bottom = Y * target.Size.Height + yRadius;
            var rectangle = new RawRectangleF(left, top, right, bottom);
            switch (Mode)
            {
                case Shape.Rectangle:
                    if (Fill)
                    {
                        target.FillRectangle(rectangle, brush);
                    }
                    else
                    {
                        target.DrawRectangle(rectangle, brush, Thickness);
                    }
                    break;

                case Shape.Ellipse:
                    var center = new RawVector2(X * target.Size.Width, Y * target.Size.Height);
                    var ellipse = new Ellipse(center, xRadius, yRadius);
                    if (Fill)
                    {
                        target.FillEllipse(ellipse, brush);
                    }
                    else
                    {
                        target.DrawEllipse(ellipse, brush, Thickness);
                    }
                    break;

                case Shape.RoundedRectangle:
                    var xCornerRadius = xRadius / 2.0f;
                    var yCornerRadius = yRadius / 2.0f;
                    if (Fill)
                    {
                        var roundedRectangle = new RoundedRectangle { Rect = rectangle, RadiusX = xCornerRadius, RadiusY = yCornerRadius };
                        target.FillRoundedRectangle(roundedRectangle, brush);
                    }
                    else
                    {
                        var roundedRectangle = new RoundedRectangle { Rect = rectangle, RadiusX = xCornerRadius, RadiusY = yCornerRadius };
                        target.DrawRoundedRectangle(roundedRectangle, brush, Thickness);
                    }
                    break;
            }
        }
    }
}
