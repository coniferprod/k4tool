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
            builder.Append(String.Format("SHAPE      ={0}\nSPEED      ={1,3}\nDEPTH      ={2,3}\nPRESS DEPTH={3,3}", 
                Enum.GetNames(typeof(LFOShape))[(int)Shape], 
                Speed, Depth - 50, Pressure - 50));
            return builder.ToString();
        }
    }
}