using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ColorOrganViewModel : PluginNode
    {
        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("ColorOrgan");
        }
    }
}
