using System;
using System.Text;

namespace k4tool
{
    public enum WheelAssign
    {
        Vibrato,
        LFO,
        DCF
    }

    public class VibratoSettings
    {
        public LFOShape Shape; // 0/TRI, 1/SAW, 2/SQR, 3/RND

        public int Speed; // 0~100

        public int Pressure; // 0~100 (±50)

        public int Depth; // 0~100 (±50)

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("shape = {0}, speed = {1}", Enum.GetNames(typeof(LFOShape))[(int)Shape], Speed));

            return builder.ToString();
        }
    }
}