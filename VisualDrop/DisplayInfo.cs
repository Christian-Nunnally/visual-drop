using System;

namespace VisualDrop
{
    [Serializable]
    public class DisplayInfo
    {
        public byte DisplaySize { get; set; }
        public DisplayInfo ChainedDisplay { get; set; }
        public IVisualEffect VisualEffect { get; set; }
    }
}