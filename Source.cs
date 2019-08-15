using System;
using System.Text;

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

        public int WaveNumber;  // combined from two bytes

        public int KeyScalingCurve;  // 0~7 / 1~8

        public int Coarse; // 0~48 / ±24

        public bool KeyTracking; 

        public int FixedKey; // 0 ~ 115 / C-1 ~ G8
        
        public int Fine; // 0~100 / ±50

        public bool PressureToFrequencySwitch; 

        public bool VibratoSwitch;

        public int VelocityCurve; // 0~7 / 1~8

        public Amplifier Amp;

        public Source()
        {
            Amp = new Amplifier();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("delay = {0}\n", Delay));
            builder.Append(String.Format("wave = {0}\n", Wave.Instance[this.WaveNumber]));
            builder.Append(String.Format("KS curve = {0}\n", KeyScalingCurve + 1));
            builder.Append(String.Format("coarse = {0}, fine = {1}\n", Coarse - 24, Fine - 50));
            builder.Append(String.Format("key tracking = {0}, fixed key = {1} ({2})\n", KeyTracking ? "ON" : "OFF", FixedKey, GetNoteName(FixedKey)));
            builder.Append(String.Format("prs>freq switch = {0}, vib>a.bend switch = {1}\n", PressureToFrequencySwitch ? "ON" : "OFF", VibratoSwitch ? "ON" : "OFF"));
            builder.Append(String.Format("velocity curve = {0}\n", VelocityCurve + 1));
            builder.Append(String.Format("DCA: {0}", Amp.ToString()));

            return builder.ToString();
        }

        public static String GetNoteName(int noteNumber) {
            String[] notes = new String[] {"A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            int octave = noteNumber / 12 + 1;
            String name = notes[noteNumber % 12];
            return name + octave;
        }
    }
}
