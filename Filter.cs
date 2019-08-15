using System;
using System.Text;

namespace k4tool
{
    public class Filter
    {
        public int Cutoff;  // 0~100

        public int Resonance; // 0 ~ 7 / 1 ~ 8

        public LevelModulation CutoffMod;

        public bool IsLFO;  // 0/off, 1/on

        public Envelope Env;

        public int EnvelopeDepth; // 0 ~ 100 (±50)

        public int EnvelopeVelocityDepth; // 0 ~ 100 (±50)

        public TimeModulation TimeMod;

        public Filter()
        {
            Env = new Envelope();
            CutoffMod = new LevelModulation();
            TimeMod = new TimeModulation();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("cutoff = {0}, resonance = {1}\n", Cutoff, Resonance));
            builder.Append(String.Format("LFO = {0}\n", IsLFO ? "ON" : "OFF"));
            builder.Append(String.Format("envelope: {0}\n", Env.ToString()));
            builder.Append(String.Format("cutoff modulation: velocity = {0}, pressure = {1}, key scaling = {2}\n", CutoffMod.VelocityDepth, CutoffMod.PressureDepth, CutoffMod.KeyScalingDepth));
            builder.Append(String.Format("time modulation: attack velocity = {0}, release velocity = {1}, key scaling = {2}\n", TimeMod.AttackVelocity, TimeMod.ReleaseVelocity, TimeMod.KeyScaling));
            return builder.ToString();
        }

    }
}