﻿using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.Serialization;

namespace DiiagramrFadeCandy
{
    [Serializable]
    public class SpectrumEffect : IGraphicEffect
    {
        [DataMember]
        public byte[] SpectrumData { get; set; }

        public void Draw(RenderTarget target)
        {
            if (SpectrumData == null)
            {
                return;
            }
            var targetWidth = target.Size.Width;
            var targetHeight = target.Size.Height;

            var barWidth = targetWidth / SpectrumData.Length;
            for (int i = 0; i < SpectrumData.Length; i++)
            {
                var brush = new SolidColorBrush(target, new RawColor4(1f, 1f, 1f, .5f));
                var left = i * barWidth;
                var top = targetHeight;
                var right = i * barWidth + barWidth;
                var bottom = targetHeight - (targetHeight / 255f * SpectrumData[i]);
                var rectangle = new RawRectangleF(left, top, right, bottom);
                target.FillRectangle(rectangle, brush);
            }
        }
    }
}
