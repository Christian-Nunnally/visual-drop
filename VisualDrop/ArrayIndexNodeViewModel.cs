using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ArrayIndexNodeViewModel : PluginNode
    {
        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("ArrayIndexNode");
            IndexTerminal = setup.InputTerminal<int>("Index", Direction.West);
            OutputTerminal = setup.OutputTerminal<byte>("Output", Direction.South);
            setup.InputTerminal<byte[]>("Array", Direction.North).DataChanged += OnDataChanged;
        }

        public Terminal<byte> OutputTerminal { get; set; }

        public Terminal<int> IndexTerminal { get; set; }

        private void OnDataChanged(byte[] data)
        {
            if (data == null || IndexTerminal.Data < 0 || IndexTerminal.Data >= data.Length) return;
            OutputTerminal.Data = data[IndexTerminal.Data];
        }
    }
}
