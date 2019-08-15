using System;
using System.Text;

namespace k4tool
{
    public class Filter
    {
        public Envelope Env;

        public int Cutoff;  // 0~100

        public int Resonance; // 0 ~ 7 / 1 ~ 8

        public bool IsLFO;  // 0/off, 1/on

        public int CutoffModVel;  // 0 ~ 100 (±50)

        public int CutoffModPrs;   // 0 ~ 100 (±50)

        public int CutoffModKS;   // 0 ~ 100 (±50)

        public int EnvelopeDepth; // 0 ~ 100 (±50)

        public int EnvelopeVelocityDepth; // 0 ~ 100 (±50)

        public int TimeModulationOnVelocity; // 0~100 (±50)

        public int TimeModulationOffVelocity; // 0~100 (±50)

        public int TimeModulationKeyScaling; // 0~100 (±50)

        public Filter()
        {
            Env = new Envelope();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("cutoff = {0}, resonance = {1}", Cutoff, Resonance));
            builder.Append(String.Format("LFO = {0}", IsLFO ? "ON" : "OFF"));
            return builder.ToString();
        }

    }
}