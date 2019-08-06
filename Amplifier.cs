namespace k4tool
{
    public class AmplifierEnvelope
    {
        public int Attack; // 0~100

        public int Decay; // 0~100

        public int Sustain; // 0~100

        public int Release; // 0~100

    }

    // Source-specific amplifier settings
    public class Amplifier
    {
        public int EnvelopeLevel; // 0~100

        public int LevelModulationVelocity; // 0~100 (±50)

        public int LevelModulationPressure; // 0~100 (±50)

        public int LevelModulationKeyScaling; // 0~100 (±50)

        public int TimeModulationOnVelocity; // 0~100 (±50)

        public int TimeModulationOffVelocity; // 0~100 (±50)

        public int TimeModulationKeyScaling; // 0~100 (±50)

    }
}