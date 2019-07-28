namespace k4tool
{
    // Source-specific amplifier settings
    public class Amplifier
    {
        public int EnvelopeLevel; // 0~100

        public int EnvelopeAttack; // 0~100

        public int EnvelopeDecay; // 0~100

        public int EnvelopeSustain; // 0~100

        public int EnvelopeRelease; // 0~100

        public int LevelModulationVelocity; // 0~100 (±50)

        public int LevelModulationPressure; // 0~100 (±50)

        public int LevelModulationKeyScaling; // 0~100 (±50)

        public int TimeModulationOnVelocity; // 0~100 (±50)

        public int TimeModulationOffVelocity; // 0~100 (±50)

        public int TimeModulationKeyScaling; // 0~100 (±50)

        
    }
}