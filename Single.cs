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

            public WheelAssign WheelAssign; // 0/VIB, 1/LFO, 2/DCF

            public int WheelDepth; // 0~100 (±50)

            public AutoBendSettings AutoBend;

            public LFOSettings LFO;

            public VibratoSettings Vibrato;

            public int PressureFreq; // 0~100 (±50)

            public CommonSettings()
            {
                Vibrato = new VibratoSettings();
                AutoBend = new AutoBendSettings();
                LFO = new LFOSettings();
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
            Common = new CommonSettings();

            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            Common.Name = GetName(data, offset);
            offset += 10;  // name is s00 to s09
            (Common.Volume, offset) = Util.GetNextByte(data, offset);

            // effect = s11 bits 0...4
            (b, offset) = Util.GetNextByte(data, offset);
            Common.Effect = (int)(b & 0x1f); // 0b00011111

            // output select = s12 bits 0...2
            (b, offset) = Util.GetNextByte(data, offset);
            Common.Output = (int)(b & 0x07); // 0b00000111

            // source mode = s13 bits 0...1
            (b, offset) = Util.GetNextByte(data, offset);
            Common.SourceMode = (SourceMode)(b & 0x03);
            Common.PolyphonyMode = (PolyphonyMode)((b >> 2) & 0x03);
            Common.AMS1ToS2 = ((b >> 4) & 0x01) == 1;
            Common.AMS3ToS4 = ((b >> 5) & 0x01) == 1;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.S1Mute = (b & 0x01) == 0;  // 0/mute, 1/not mute
            Common.S2Mute = ((b >> 1) & 0x01) == 0;  // 0/mute, 1/not mute
            Common.S3Mute = ((b >> 2) & 0x01) == 0;  // 0/mute, 1/not mute
            Common.S4Mute = ((b >> 3) & 0x01) == 0;  // 0/mute, 1/not mute

            Common.Vibrato.Shape = (LFOShape)((b >> 4) & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            // Pitch bend = s15 bits 0...3
            Common.PitchBend = (int)(b & 0x0f);
            // Wheel assign = s15 bits 4...5
            Common.WheelAssign = (WheelAssign)((b >> 4) & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            // Vibrato speed = s16 bits 0...6
            Common.Vibrato.Speed = b & 0x7f;

            // Wheel depth = s17 bits 0...6
            (b, offset) = Util.GetNextByte(data, offset);
            Common.WheelDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.AutoBend.Time = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.AutoBend.Depth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.AutoBend.KeyScalingTime = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.AutoBend.VelocityDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.LFO.Shape = (LFOShape)(b & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            Common.LFO.Speed = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.LFO.Delay = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.LFO.Depth = b & 0x7f;
            
            (b, offset) = Util.GetNextByte(data, offset);
            Common.LFO.PressureDepth = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Common.PressureFreq = b & 0x7f;
        }

        private string GetName(byte[] data, int offset)
        {
            // Brute-force the name in s0 ... s9
            byte[] bytes = { data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9] };
        	return Encoding.ASCII.GetString(bytes);
        }

        private string GetSourceMuteString(bool s1, bool s2, bool s3, bool s4)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(s1 ? "1" : "-");
            builder.Append(s2 ? "2" : "-");
            builder.Append(s3 ? "3" : "-");
            builder.Append(s4 ? "4" : "-");
            return builder.ToString();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("{0}\nVolume = {1}\nEffect = {2}\nOutput = {3}\n", Common.Name, Common.Volume, Common.Effect + 1, "ABCDEFGH"[Common.Output]));
            builder.Append(String.Format("Source mode = {0}\n", Enum.GetNames(typeof(SourceMode))[(int)Common.SourceMode]));
            builder.Append(String.Format("Polyphony mode = {0}\n", Enum.GetNames(typeof(PolyphonyMode))[(int)Common.PolyphonyMode]));
            builder.Append(String.Format("AM 1>2 = {0}\nAM 3>4 = {1}\n", Common.AMS1ToS2 ? "ON" : "OFF", Common.AMS3ToS4 ? "ON" : "OFF"));
            builder.Append(String.Format("Source mutes: {0}\n", GetSourceMuteString(Common.S1Mute, Common.S2Mute, Common.S3Mute, Common.S4Mute)));
            builder.Append(String.Format("Vibrato: {0}\n", Common.Vibrato.ToString()));
            builder.Append(String.Format("Pitch bend: {0}\n", Common.PitchBend));
            builder.Append(String.Format("Wheel: assign = {0}, depth = {1}\n", Enum.GetNames(typeof(WheelAssign))[(int)Common.WheelAssign], Common.WheelDepth - 50));
            builder.Append(String.Format("Auto bend: {0}\n", Common.AutoBend.ToString()));
            builder.Append(String.Format("LFO: {0}\n", Common.LFO.ToString()));
            builder.Append(String.Format("Pres>Freq = {0}\n", Common.PressureFreq));
            return builder.ToString();
        }

    }
    
}