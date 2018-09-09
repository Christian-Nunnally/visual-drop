using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ComposeEffectViewModel : PluginNode
    {
        private readonly CompositeVisualEffect _compositeEffect = new CompositeVisualEffect(3);

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(80, 40);
            setup.NodeName("ComposeEffect");

            Effect1 = setup.InputTerminal<IVisualEffect>("Effect 1", Direction.North);
            Effect2 = setup.InputTerminal<IVisualEffect>("Effect 2", Direction.North);
            Effect3 = setup.InputTerminal<IVisualEffect>("Effect 3", Direction.North);

            OutputEffect = setup.OutputTerminal<IVisualEffect>("Composed Effect", Direction.South);

            Effect1.DataChanged += Effect1OnDataChanged;
            Effect2.DataChanged += Effect2OnDataChanged;
            Effect3.DataChanged += Effect3OnDataChanged;

            OutputEffect.Data = _compositeEffect;
        }

        public Terminal<IVisualEffect> OutputEffect { get; set; }

        private void Effect1OnDataChanged(IVisualEffect data)
        {
            _compositeEffect.SetEffect(0, data);
        }

        private void Effect2OnDataChanged(IVisualEffect data)
        {
            _compositeEffect.SetEffect(2, data);
        }

        private void Effect3OnDataChanged(IVisualEffect data)
        {
            _compositeEffect.SetEffect(2, data);
        }

        public Terminal<IVisualEffect> Effect1 { get; set; }
        public Terminal<IVisualEffect> Effect2 { get; set; }
        public Terminal<IVisualEffect> Effect3 { get; set; }
    }
}
