using System;
using System.Text;

namespace k4tool
{
    // Source-specific amplifier settings
    public class Amplifier
    {
        public Envelope Env;

        public int EnvelopeLevel; // 0~100

        public int LevelModulationVelocity; // 0~100 (±50)

        public int LevelModulationPressure; // 0~100 (±50)

        public int LevelModulationKeyScaling; // 0~100 (±50)

        public int TimeModulationOnVelocity; // 0~100 (±50)

        public int TimeModulationOffVelocity; // 0~100 (±50)

        public int TimeModulationKeyScaling; // 0~100 (±50)

        public Amplifier()
        {
            Env = new Envelope(0, 0, 0, 0);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("envelope = {0}, level = {1}\n", Env.ToString(), EnvelopeLevel));
            builder.Append(String.Format("level modulation: velocity = {0}, pressure = {1}, key scaling = {2}\n", LevelModulationVelocity, LevelModulationPressure, LevelModulationKeyScaling));
            builder.Append(String.Format("time modulation: on velocity = {0}, off velocity = {1}, key scaling = {2}\n", TimeModulationOnVelocity, TimeModulationOffVelocity, TimeModulationKeyScaling));
            return builder.ToString();
        }
    }
}