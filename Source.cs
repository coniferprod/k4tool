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
            builder.Append("S.COMMON\n");
            builder.Append(String.Format("DELAY      ={0,3}\n", Delay));
            builder.Append(String.Format("VEL CURVE  ={0,3}\n", VelocityCurve + 1));
            builder.Append(String.Format("KS CURVE   ={0,3}\n", KeyScalingCurve + 1));
            builder.Append("DCO\n");
            builder.Append(String.Format("WAVE       ={0,3} ({1})\n", WaveNumber + 1, Wave.Instance[WaveNumber]));
            builder.Append(String.Format("KEY TRACK  ={0}\n", KeyTracking ? "ON" : "OFF"));
            builder.Append(String.Format("COARSE     ={0,3}\nFINE       ={1,3}\n", Coarse - 24, Fine - 50));
            builder.Append(String.Format("FIXED KEY  ={0} ({1})\n", GetNoteName(FixedKey), FixedKey));
            builder.Append(String.Format("PRESS      ={0}\nVIB/A.BEND ={1}\n", PressureToFrequencySwitch ? "ON" : "OFF", VibratoSwitch ? "ON" : "OFF"));
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
