using DiiagramrAPI.Diagram;

namespace VisualDrop
{
    public class SumArrayViewModel : Node
    {
        public TypedTerminal<byte[]> ArrayInputTerminal { get; private set; }
        public TypedTerminal<float> SumOutputTerminal { get; private set; }

        protected override void SetupNode(NodeSetup setup)
        {
            setup.NodeName("Sum Array");
            setup.NodeSize(30, 30);
            ArrayInputTerminal = setup.InputTerminal<byte[]>("Array In", Direction.North);
            SumOutputTerminal = setup.OutputTerminal<float>("Sum of Array", Direction.South);

            ArrayInputTerminal.DataChanged += ArrayInputTerminal_DataChanged;
        }

        private void ArrayInputTerminal_DataChanged(byte[] array)
        {
            if (array != null)
            {
                float sum = 0;
                foreach (var data in array)
                {
                    sum += data;
                }
                SumOutputTerminal.Data = sum;
            }
        }
    }
}
