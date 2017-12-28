using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class OnOffEffectViewModel : PluginNode
    {
        private readonly VisualEffect _visualEffect = new VisualEffect(2);
        
        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("OnOffEffect");
            setup.InputTerminal<bool>("On/Off", Direction.North).DataChanged += OnDataChanged;
            OnGraphicTerminal = setup.InputTerminal<byte[]>("On Graphic", Direction.East);
            var outputTerminal = setup.OutputTerminal<IVisualEffect>("Effect", Direction.South);

            _visualEffect.SetFrame(new byte[] {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}, 0);
            _visualEffect.SetFrame(new byte[] {255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255}, 1);

            OnGraphicTerminal.DataChanged += OnGraphicTerminalOnDataChanged;
            outputTerminal.Data = _visualEffect;
        }

        private void OnGraphicTerminalOnDataChanged(byte[] data)
        {
            if (data == null) return;
            if (_visualEffect.Frames[0].Length != data.Length) _visualEffect.SetFrame(new byte[data.Length], 0);
            _visualEffect.SetFrame(data, 1);
        }

        public Terminal<byte[]> OnGraphicTerminal { get; set; }

        private void OnDataChanged(bool data)
        {
            _visualEffect.CurrentFrame = data ? 1 : 0;
        }
    }
}
