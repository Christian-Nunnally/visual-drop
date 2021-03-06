using System;
using DiiagramrAPI.PluginNodeApi;

namespace VisualDrop
{
    public class ArrayIndexNodeViewModel : PluginNode
    {
        public int Index { get; set; }

        private int _lastInputArrayLength = 0;

        public override void SetupNode(NodeSetup setup)
        {
            setup.NodeSize(40, 40);
            setup.NodeName("ArrayIndexNode");
            OutputTerminal = setup.OutputTerminal<byte>("Output", Direction.South);
            setup.InputTerminal<byte[]>("Array", Direction.North).DataChanged += OnDataChanged;
        }

        public Terminal<byte> OutputTerminal { get; set; }

        private void OnDataChanged(byte[] data)
        {
            if (data == null)
            {
                return;
            }
             
            _lastInputArrayLength = data.Length; 
            if (Index >= data.Length)
            {
                Index = _lastInputArrayLength;
                return;
            }
            
            OutputTerminal.Data = data[Index];
        }

        public void Add()
        {
            if (Index < _lastInputArrayLength - 1) Index++;
        }

        public void Subtract()
        {
            if (Index > 0) Index--;
        }
    }
}
