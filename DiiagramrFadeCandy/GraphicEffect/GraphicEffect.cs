using DiiagramrAPI.Diagram;
using SharpDX.Direct2D1;

namespace DiiagramrFadeCandy
{
    public class GraphicEffect : IWireableType
    {
        public virtual void Draw(RenderTarget target) { }

        public System.Windows.Media.Color GetTypeColor()
        {
            return new System.Windows.Media.Color() { R = 85, G = 85, B = 150, A = 255 };
        }
    }
}
