namespace k4tool
{
    public class VibratoSettings
    {
        public LFOShape Shape; // 0/TRI, 1/SAW, 2/SQR, 3/RND

        public int Speed; // 0~100

        public int Pressure; // 0~100 (±50)

        public int Depth; // 0~100 (±50)
    }
}