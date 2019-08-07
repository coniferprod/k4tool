namespace k4tool
{
    public enum LFOShape // 0/TRI, 1/SAW, 2/SQR, 3/RND
    {
        Triangle,
        Sawtooth,
        Square,
        Random
    };

    public class LFOSettings
    {
        public LFOShape Shape;

        public int Speed;  // 0~100

        public int Delay;  // 0~100

        public int Depth; // 0~100 (±50)
        
        public int PressureDepth; // 0~100 (±50)        
    }
}