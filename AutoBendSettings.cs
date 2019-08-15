using System;
using System.Text;

namespace k4tool
{
    public class AutoBendSettings
    {
        public int Time;  // 0~100

        public int Depth; // 0~100 (±50)

        public int KeyScalingTime; // 0~100 (±50)

        public int VelocityDepth; // 0~100 (±50)

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("TIME       ={0,3}\nDEPTH      ={1,2}\nKS>TIME    ={2,2}\nVEL>DEPTH  ={3,2}", Time, Depth - 50, KeyScalingTime - 50, VelocityDepth - 50));
            return builder.ToString();
        }
    }
}