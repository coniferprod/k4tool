namespace k4tool
{
    public enum SourceMode
    {
        Normal,
        Twin,
        Double
    };

    public class Source
    {
        public int Delay;  // 0~100

        public int Wave;  // combined from two bytes

        public int KeyScalingCurve;  // 0~7 / 1~8

        public int Coarse; // 0~48 / ±24

        public bool KeyTracking; 

        public int FixedKey; // 0 ~ 115 / C-1 ~ G8
        
        public int Fine; // 0~100 / ±50

        public bool PressureToFrequencySwitch; 

        public bool VibratoSwitch;

        public int VelocityCurve; // 0~7 / 1~8

        public Amplifier Amp;

        public Filter Filter1;
        public Filter Filter2;
    }
}
