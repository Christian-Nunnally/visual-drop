using System.Collections.Generic;
using System.Windows.Media;
using ColorOrgan5Nodes.NodeTools;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class LedDisplayViewModel : PluginNode
    {
        public bool flip { get; set; }

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("LedDisplay");
            OutputTerminal = setup.OutputTerminal<byte[]>("Graphic Out", Direction.South);
        }

        public Terminal<byte[]> OutputTerminal { get; set; }

        public void SendGraphic()
        {
            var graphic = new StaticLedGraphic();
            var graphic2 = new StaticLedGraphic(); 

            for (int i = 0; i < 64; i++)
            {
                graphic.SetPixel(i, Colors.BlueViolet);
            }

            for (int i = 0; i < 64; i++)
            {
                graphic2.SetPixel(i, Colors.Red);
            }

            OutputTerminal.Data = flip ? graphic.Graphic : graphic2.Graphic;
            flip = !flip;
        }
    }
}
