using DiagramEditor.ViewModel.Diagram;

namespace ColorOrgan5Nodes.ExampleNodes
{
    public class PluginDemoViewModel : PluginNodeViewModel
    {
        public override string Name => "Plugin Demo";

        public override void NodeLoaded()
        {

        }

        public override void ConstructTerminals()
        {
            ConstructNewInputTerminal("Input", typeof(int), Direction.South, "");
            ConstructNewInputTerminal("Input", typeof(int), Direction.North, "");
            ConstructNewOutputTerminal("Output", typeof(int), Direction.East);
            ConstructNewOutputTerminal("Output", typeof(int), Direction.West);
        }
    }
}
