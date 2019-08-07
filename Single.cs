using System;
using System.Text;

namespace k4tool
{
    public class Single
    {
        public enum PolyphonyMode
        {
            Poly1,
            Poly2,
            Solo1,
            Solo2
        };

        public class CommonSettings
        {
            public string Name;
            
            public int Volume;  // 0~100

            public int Effect;  // 0~31 / 1~32

            public int Output; // 0~7 / A~H

            public SourceMode SourceMode;

            public PolyphonyMode PolyphonyMode;

            public bool AMS1ToS2;
            public bool AMS3ToS4;

            public bool S1Mute;
            public bool S2Mute;
            public bool S3Mute;
            public bool S4Mute;

            public int PitchBend;  // 0~12

            public int WheelAssign; // 0/VIB, 1/LFO, 2/DCF

            public int WheelDepth; // 0~100 (±50)

            public AutoBendSettings AutoBend;

            public LFOSettings LFO;

            public VibratoSettings Vibrato;

            public int PressureFreq; // 0~100 (±50)

            public CommonSettings()
            {

            }            
        }

        private CommonSettings Common;

        const int NumSources = 4;

        public Source[] Sources;

        public Single()
        {
            Sources = new Source[NumSources];
            
        }

        public Single(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            Common.Name = GetName(data, offset);
        }

        private string GetName(byte[] data, int offset)
        {
            // Brute-force the name in s0 ... s9
            byte[] bytes = { data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9] };
        	return Encoding.ASCII.GetString(bytes);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("*SINGLE* {0}\n\n", Common.Name));
            return builder.ToString();
        }

    }
    
}