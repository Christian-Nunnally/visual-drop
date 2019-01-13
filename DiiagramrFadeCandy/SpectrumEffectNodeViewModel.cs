using DiiagramrAPI.PluginNodeApi;

namespace DiiagramrFadeCandy
{
    public class SpectrumEffectNodeViewModel : PluginNode
    {
        public SpectrumEffect SpectrumEffect { get; set; } = new SpectrumEffect();

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeName("Spectrum Visualizer");
            setup.NodeSize(30, 30);

            setup.InputTerminal<byte[]>("Signal", Direction.North).DataChanged += SignalInputChanged;
            setup.OutputTerminal<IGraphicEffect>("Effect", Direction.South).Data = SpectrumEffect;
        }

        private void SignalInputChanged(byte[] data)
        {
            if (data != null)
            {
                SpectrumEffect.SpectrumData = data;
            }
        }
    }
}
