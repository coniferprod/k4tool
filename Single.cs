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

        public class Common
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

            public Common()
            {

            }            
        }

        private Common CommonParams;

        const int NumSources = 4;

        public Source[] Sources;

        public Single()
        {
            Sources = new Source[NumSources];
            
        }

    }
    
}