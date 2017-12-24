namespace VisualDrop
{
    public class VisualEffect : IVisualEffect
    {
        public byte[][] Frames { get; set; }
        public int CurrentFrame { get; set; }
        private int _numberOfFrames;

        public VisualEffect(int numberOfFrames)
        {
            _numberOfFrames = numberOfFrames;
            Frames = new byte[numberOfFrames][];
        }

        public void AddFrame(byte[] frame)
        {
            Frames[CurrentFrame++] = frame;
        }

        public byte[] GetEffect()
        {
            if (CurrentFrame >= 0 && CurrentFrame < _numberOfFrames) return Frames[CurrentFrame];
            return null;
        }
    }
}