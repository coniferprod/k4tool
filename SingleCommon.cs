namespace k4tool
{
    public class SingleCommon
    {
        public string Name;
        
        public int Volume;  // 0~100

        public int Effect;  // 0~31 / 1~32

        public int OutSelect; // 0~7 / A~H

        public int SourceMode; // 0/NORM, 1/TWIN, 2/DBL

        public int PolyMode; // 0/PL1, 1/PL2, 2/SOLO1, 3/SOLO2

        public bool AMS1ToS2;
        public bool AMS3ToS4;

        public bool S1Mute;
        public bool S2Mute;
        public bool S3Mute;
        public bool S4Mute;

        public int VibratoShape; // 0/TRI, 1/SAW, 2/SQR, 3/RND

        public int PitchBend;  // 0~12
        public int WheelAssign; // 0/VIB, 1/LFO, 2/DCF

        public int VibratoSpeed; // 0~100

        public int WheelDepth; // 0~100 (±50)

        public int AutoBendTime;  // 0~100

        public int AutoBendDepth; // 0~100 (±50)

        public int AutoBendKSTime; // 0~100 (±50)

        public int AutoBendVelDep; // 0~100 (±50)

        public int VibratoPressure; // 0~100 (±50)

        public int VibratoDepth; // 0~100 (±50)

        public int LFOShape; // 0/TRI, 1/SAW, 2/SQR, 3/RND

        public int LFOSpeed;  // 0~100

        public int LFODelay;  // 0~100

        public int LFODepth; // 0~100 (±50)

        public int LFOPressureDepth; // 0~100 (±50)

        public int PressureFreq; // 0~100 (±50)
        

        
    }
}