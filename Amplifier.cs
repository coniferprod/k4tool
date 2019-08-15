using System;
using System.Text;

namespace k4tool
{
    public class LevelModulation
    {
        public int VelocityDepth; // 0~100 (±50)

        public int PressureDepth; // 0~100 (±50)

        public int KeyScalingDepth; // 0~100 (±50)

        public LevelModulation()
        {
            VelocityDepth = 0;
            PressureDepth = 0;
            KeyScalingDepth = 0;
        }
        
        public LevelModulation(int vel, int prs, int ks)
        {
            VelocityDepth = vel;
            PressureDepth = prs;
            KeyScalingDepth = ks;
        }
    }

    public class TimeModulation
    {
        public int AttackVelocity; // 0~100 (±50)
        public int ReleaseVelocity; // 0~100 (±50)
        public int KeyScaling; // 0~100 (±50)

        public TimeModulation()
        {
            AttackVelocity = 0;
            ReleaseVelocity = 0;
            KeyScaling = 0;
        }

        public TimeModulation(int a, int r, int ks)
        {
            AttackVelocity = a;
            ReleaseVelocity = r;
            KeyScaling = ks;
        }
    }

    // Source-specific amplifier settings
    public class Amplifier
    {
        public Envelope Env;

        public int EnvelopeLevel; // 0~100

        public LevelModulation LevelMod;
        public TimeModulation TimeMod;

        public Amplifier()
        {
            Env = new Envelope(0, 0, 0, 0);
            LevelMod = new LevelModulation(0, 0, 0);
            TimeMod = new TimeModulation(0, 0, 0);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("envelope = {0}, level = {1}\n", Env.ToString(), EnvelopeLevel));
            builder.Append(String.Format("level modulation: velocity = {0}, pressure = {1}, key scaling = {2}\n", LevelMod.VelocityDepth, LevelMod.PressureDepth, LevelMod.KeyScalingDepth));
            builder.Append(String.Format("time modulation: attack velocity = {0}, release velocity = {1}, key scaling = {2}\n", TimeMod.AttackVelocity, TimeMod.ReleaseVelocity, TimeMod.KeyScaling));
            return builder.ToString();
        }
    }
}