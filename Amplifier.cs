using System;
using System.Text;
using System.Collections.Generic;

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

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)VelocityDepth);
            data.Add((byte)PressureDepth);
            data.Add((byte)KeyScalingDepth);
            return data.ToArray();
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

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)AttackVelocity);
            data.Add((byte)ReleaseVelocity);
            data.Add((byte)KeyScaling);
            return data.ToArray();
        }
    }

    // Source-specific amplifier settings
    public class Amplifier
    {
        public const int DataSize = 11;

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

        public Amplifier(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            Env = new Envelope(0, 0, 0, 0);
            LevelMod = new LevelModulation(0, 0, 0);
            TimeMod = new TimeModulation(0, 0, 0);

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeLevel = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Attack = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Decay = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Sustain = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Release = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            LevelMod.VelocityDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            LevelMod.PressureDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            LevelMod.KeyScalingDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            TimeMod.AttackVelocity = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            TimeMod.ReleaseVelocity = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            TimeMod.KeyScaling = b & 0x7f;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("envelope = {0}, level = {1}\n", Env.ToString(), EnvelopeLevel));
            builder.Append(String.Format("level modulation: velocity = {0}, pressure = {1}, key scaling = {2}\n", LevelMod.VelocityDepth, LevelMod.PressureDepth, LevelMod.KeyScalingDepth));
            builder.Append(String.Format("time modulation: attack velocity = {0}, release velocity = {1}, key scaling = {2}\n", TimeMod.AttackVelocity, TimeMod.ReleaseVelocity, TimeMod.KeyScaling));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)EnvelopeLevel);
            data.AddRange(Env.ToData());
            data.AddRange(LevelMod.ToData());
            data.AddRange(TimeMod.ToData());
            return data.ToArray();
        }
    }
}