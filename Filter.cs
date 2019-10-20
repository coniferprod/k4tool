using System;
using System.Text;
using System.Collections.Generic;

namespace k4tool
{
    public class Filter
    {
        public const int DataSize = 28;

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
            Cutoff = 99;
            Resonance = 0;
            CutoffMod = new LevelModulation();
            IsLFO = false;
            Env = new Envelope();
            EnvelopeDepth = 0;
            EnvelopeVelocityDepth = 0;
            TimeMod = new TimeModulation();
        }

        public Filter(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);
            Cutoff = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Resonance = b & 0x07;
            IsLFO = b.IsBitSet(3);

            CutoffMod = new LevelModulation();
            (b, offset) = Util.GetNextByte(data, offset);
            CutoffMod.VelocityDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            CutoffMod.PressureDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            CutoffMod.KeyScalingDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeVelocityDepth = b & 0x7f;

            Env = new Envelope();
            (b, offset) = Util.GetNextByte(data, offset);
            Env.Attack = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Decay = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Env.Sustain = b & 0x7f;
            
            (b, offset) = Util.GetNextByte(data, offset);
            Env.Release = b & 0x7f;

            TimeMod = new TimeModulation();
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
            builder.Append(String.Format("cutoff = {0}, resonance = {1}\n", Cutoff, Resonance));
            builder.Append(String.Format("LFO = {0}\n", IsLFO ? "ON" : "OFF"));
            builder.Append(String.Format("envelope: {0}\n", Env.ToString()));
            builder.Append(String.Format("cutoff modulation: velocity = {0}, pressure = {1}, key scaling = {2}\n", CutoffMod.VelocityDepth, CutoffMod.PressureDepth, CutoffMod.KeyScalingDepth));
            builder.Append(String.Format("time modulation: attack velocity = {0}, release velocity = {1}, key scaling = {2}\n", TimeMod.AttackVelocity, TimeMod.ReleaseVelocity, TimeMod.KeyScaling));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
                        
            return data.ToArray();
        }
    }
}