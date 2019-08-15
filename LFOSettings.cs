using System;
using System.Text;

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

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("SHAPE      ={0}\nSPEED     ={1,3}\nDELAY     ={2,3}\nDEPTH     ={3,3}, PRESS DEPTH={4,3}", 
                Enum.GetNames(typeof(LFOShape))[(int)Shape],
                Speed, Delay, Depth - 50, PressureDepth - 50));
            return builder.ToString();
        }    
    }
}