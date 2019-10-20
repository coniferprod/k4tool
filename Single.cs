using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

namespace k4tool
{
    public enum SourceMode
    {
        Normal,
        Twin,
        Double
    };

    public enum PolyphonyMode
    {
        Poly1,
        Poly2,
        Solo1,
        Solo2
    };

    public class CommonSettings
    {
        public const int DataSize = 30;
        private const string OutputNames = "ABCDEFGH";

        public int Volume;  // 0~100

        public int Effect;  // 0~31 / 1~32 (on K4)

        public char Output; // 0~7 / A~H (on K4r)

        public string Name;
        
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

        public AutoBendSettings AutoBend;  // same as portamento?

        public LFOSettings LFO;

        public VibratoSettings Vibrato;

        public int PressureFreq; // 0~100 (±50)

        public CommonSettings()
        {
            Volume = 80;
            Effect = 1;  // use range 1~32
            Output = 'A';
            Name = "NewSound";
            SourceMode = SourceMode.Normal;
            PolyphonyMode = PolyphonyMode.Poly1;
            AMS1ToS2 = false;
            AMS3ToS4 = false;
            PitchBend = 2;
            WheelAssign = WheelAssign.Vibrato;
            WheelDepth = 0;
            Vibrato = new VibratoSettings();
            LFO = new LFOSettings();
            AutoBend = new AutoBendSettings();
            PressureFreq = 0;
        }

        public CommonSettings(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            Name = GetName(data, offset);
            offset += 10;  // name is s00 to s09
            (b, offset) = Util.GetNextByte(data, offset);
            Volume = b;

            // effect = s11 bits 0...4
            (b, offset) = Util.GetNextByte(data, offset);
            Effect = (int)(b & 0x1f) + 1; // 0b00011111
            // use range 1~32 when storing the value, 0~31 in SysEx data

            // output select = s12 bits 0...2
            (b, offset) = Util.GetNextByte(data, offset);
            int outputNameIndex = (int)(b & 0x07); // 0b00000111;
            Output = OutputNames[outputNameIndex];

            // source mode = s13 bits 0...1
            (b, offset) = Util.GetNextByte(data, offset);
            SourceMode = (SourceMode)(b & 0x03);
            PolyphonyMode = (PolyphonyMode)((b >> 2) & 0x03);
            AMS1ToS2 = ((b >> 4) & 0x01) == 1;
            AMS3ToS4 = ((b >> 5) & 0x01) == 1;

            (b, offset) = Util.GetNextByte(data, offset);
            S1Mute = (b & 0x01) == 0;  // 0/mute, 1/not mute
            S2Mute = ((b >> 1) & 0x01) == 0;  // 0/mute, 1/not mute
            S3Mute = ((b >> 2) & 0x01) == 0;  // 0/mute, 1/not mute
            S4Mute = ((b >> 3) & 0x01) == 0;  // 0/mute, 1/not mute

            Vibrato = new VibratoSettings();
            Vibrato.Shape = (LFOShape)((b >> 4) & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            // Pitch bend = s15 bits 0...3
            PitchBend = (int)(b & 0x0f);
            // Wheel assign = s15 bits 4...5
            WheelAssign = (WheelAssign)((b >> 4) & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            // Vibrato speed = s16 bits 0...6
            Vibrato.Speed = b & 0x7f;

            // Wheel depth = s17 bits 0...6
            (b, offset) = Util.GetNextByte(data, offset);
            WheelDepth = (b & 0x7f) - 50;  // 0~100 to ±50

            AutoBend = new AutoBendSettings();
            (b, offset) = Util.GetNextByte(data, offset);
            AutoBend.Time = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            AutoBend.Depth = (b & 0x7f) - 50; // 0~100 to ±50

            (b, offset) = Util.GetNextByte(data, offset);
            AutoBend.KeyScalingTime = (b & 0x7f) - 50; // 0~100 to ±50

            (b, offset) = Util.GetNextByte(data, offset);
            AutoBend.VelocityDepth = (b & 0x7f) - 50; // 0~100 to ±50

            (b, offset) = Util.GetNextByte(data, offset);
            Vibrato.Pressure = (b & 0x7f) - 50; // 0~100 to ±50

            (b, offset) = Util.GetNextByte(data, offset);
            Vibrato.Depth = (b & 0x7f) - 50; // 0~100 to ±50

            LFO = new LFOSettings();

            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Shape = (LFOShape)(b & 0x03);

            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Speed = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Delay = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Depth = (b & 0x7f) - 50; // 0~100 to ±50
            
            (b, offset) = Util.GetNextByte(data, offset);
            LFO.PressureDepth = (b & 0x7f) - 50; // 0~100 to ±50

            (b, offset) = Util.GetNextByte(data, offset);
            PressureFreq = (b & 0x7f) - 50; // 0~100 to ±50
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
            builder.Append(String.Format("VOLUME     ={0,3}\nEFFECT PACH= {1,2}\nSUBMIX CH  =  {2}\n", Volume, Effect + 1, Output));
            builder.Append(String.Format("SOURCE MODE={0}\n", Enum.GetNames(typeof(SourceMode))[(int)SourceMode]));
            builder.Append(String.Format("AM 1>2     ={0}\nAM 3>4     ={1}\n", AMS1ToS2 ? "ON" : "OFF", AMS3ToS4 ? "ON" : "OFF"));
            builder.Append(String.Format("POLY MODE  ={0}\n", Enum.GetNames(typeof(PolyphonyMode))[(int)PolyphonyMode]));
            builder.Append(String.Format("BNDR RANGE = {0,2}\n", PitchBend));
            builder.Append(String.Format("PRESS>FREQ = {0,2}\n", PressureFreq));
            builder.Append(String.Format("WHEEL\nASSIGN     ={0}\nDEPTH      ={1,2}\n", Enum.GetNames(typeof(WheelAssign))[(int)WheelAssign], WheelDepth - 50));
            builder.Append(String.Format("AUTO BEND\n{0}\n", AutoBend.ToString()));
            builder.Append(String.Format("Sources: {0}\n", GetSourceMuteString(S1Mute, S2Mute, S3Mute, S4Mute)));
            builder.Append(String.Format("VIBRATO\n{0}\n", Vibrato.ToString()));
            builder.Append(String.Format("LFO\n{0}\n", LFO.ToString()));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add((byte)Volume);
            data.Add((byte)(Effect - 1));  // use range 0~31 in SysEx data
            data.Add((byte)(OutputNames.IndexOf(Output)));  // convert 'A', 'B' ... 'H' to 0~7

            // s13 combines source mode, poly mode, and source AM into one byte.
            // Construct a bit string, then convert it to byte.
            StringBuilder b13 = new StringBuilder("00");
            b13.Append(AMS3ToS4 ? "1" : "0");
            b13.Append(AMS1ToS2 ? "1" : "0");
            b13.Append(Convert.ToString((byte)PolyphonyMode, 2).PadLeft(2, '0'));
            b13.Append(Convert.ToString((byte)SourceMode, 2).PadLeft(2, '0'));
            data.Add(Convert.ToByte(b13.ToString(), 2));

            // s14 combines vibrato shape and source mutes into one byte.
            StringBuilder b14 = new StringBuilder("00");
            b14.Append(Convert.ToString((byte)Vibrato.Shape, 2).PadLeft(2, '0'));
            b14.Append(S4Mute ? "1" : "0");
            b14.Append(S3Mute ? "1" : "0");
            b14.Append(S2Mute ? "1" : "0");
            b14.Append(S1Mute ? "1" : "0");
            data.Add(Convert.ToByte(b14.ToString(), 2));

            // s15 combines pitch bend and wheel assign into one byte.
            StringBuilder b15 = new StringBuilder("");
            b15.Append(Convert.ToString((byte)WheelAssign, 2).PadLeft(4, '0'));
            b15.Append(Convert.ToString((byte)PitchBend, 2).PadLeft(4, '0'));
            data.Add(Convert.ToByte(b15.ToString(), 2));

            data.Add((byte)Vibrato.Speed);
            data.Add((byte)(WheelDepth + 50));  // ±50 to 0...100
            data.Add((byte)AutoBend.Time);
            data.Add((byte)(AutoBend.Depth + 50)); // ±50 to 0...100
            data.Add((byte)(AutoBend.KeyScalingTime + 50)); // ±50 to 0...100
            data.Add((byte)(AutoBend.VelocityDepth + 50)); // ±50 to 0...100
            data.Add((byte)(Vibrato.Pressure + 50)); // ±50 to 0...100
            data.Add((byte)(Vibrato.Depth + 50)); // ±50 to 0...100
            data.Add((byte)LFO.Shape);
            data.Add((byte)LFO.Speed);
            data.Add((byte)LFO.Delay);
            data.Add((byte)(LFO.Depth + 50)); // ±50 to 0...100
            data.Add((byte)(LFO.PressureDepth + 50)); // ±50 to 0...100
            data.Add((byte)(PressureFreq + 50)); // ±50 to 0...100

            var arr = data.ToArray();
            Debug.WriteLine(String.Format("Common settings = {0} bytes", arr.Length));
            return arr;
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
    }

    public class Single
    {
        public CommonSettings Common;

        const int NumSources = 4;

        public Source[] Sources;
        public Amplifier[] Amplifiers;

        public Filter Filter1;
        public Filter Filter2;

        public byte Checksum;

        public Single()
        {
            Common = new CommonSettings();
            Sources = new Source[NumSources];
            Amplifiers = new Amplifier[NumSources];
            Filter1 = new Filter();
            Filter2 = new Filter();
            Checksum = 0;
        }

        public Single(byte[] data)
        {
            //System.Console.WriteLine(String.Format("Starting to parse single patch from data (length = {0})", data.Length));

            int offset = 0;
            Common = new CommonSettings(data);
            offset += CommonSettings.DataSize;

            byte[] allSourceData = new byte[Source.DataSize * 4];
            Array.Copy(data, offset, allSourceData, 0, Source.DataSize * 4);

            List<byte> source1Data = Util.EveryNthElement(new List<byte>(allSourceData), 4, 0);
            List<byte> source2Data = Util.EveryNthElement(new List<byte>(allSourceData), 4, 1);
            List<byte> source3Data = Util.EveryNthElement(new List<byte>(allSourceData), 4, 2);
            List<byte> source4Data = Util.EveryNthElement(new List<byte>(allSourceData), 4, 3);
            
            Sources = new Source[NumSources];
            Sources[0] = new Source(source1Data.ToArray());
            Sources[1] = new Source(source2Data.ToArray());
            Sources[2] = new Source(source3Data.ToArray());
            Sources[3] = new Source(source4Data.ToArray());

            offset += Source.DataSize * 4;

            byte[] allAmpData = new byte[Amplifier.DataSize * 4];
            Array.Copy(data, offset, allAmpData, 0, Amplifier.DataSize * 4);
            List<byte> amp1Data = Util.EveryNthElement(new List<byte>(allAmpData), 4, 0);
            List<byte> amp2Data = Util.EveryNthElement(new List<byte>(allAmpData), 4, 1);
            List<byte> amp3Data = Util.EveryNthElement(new List<byte>(allAmpData), 4, 2);
            List<byte> amp4Data = Util.EveryNthElement(new List<byte>(allAmpData), 4, 3);
            Amplifiers = new Amplifier[NumSources];
            Amplifiers[0] = new Amplifier(amp1Data.ToArray());
            Amplifiers[1] = new Amplifier(amp2Data.ToArray());
            Amplifiers[2] = new Amplifier(amp3Data.ToArray());
            Amplifiers[3] = new Amplifier(amp4Data.ToArray());

            offset += Amplifier.DataSize * 4;

            // DCF
            byte[] allFilterData = new byte[Filter.DataSize * 2];
            Array.Copy(data, offset, allFilterData, 0, Filter.DataSize * 2);
            List<byte> filter1Data = Util.EveryNthElement(new List<byte>(allFilterData), 2, 0);
            List<byte> filter2Data = Util.EveryNthElement(new List<byte>(allFilterData), 2, 1);
            Filter1 = new Filter(filter1Data.ToArray());
            Filter2 = new Filter(filter2Data.ToArray());
            offset += Filter.DataSize * 2;

            byte b = 0;  // will be reused when getting the next byte
            (b, offset) = Util.GetNextByte(data, offset);
            // "Check sum value (s130) is the sum of the A5H and s0 ~ s129".
            Checksum = b;

            byte sum = ComputeChecksum();
            if (Checksum != sum)
            {
                System.Console.WriteLine(String.Format("CHECKSUM ERROR! Expected {0}, got {1}", Checksum, sum));
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Common.ToString());
            for (int i = 0; i < NumSources; i++)
            {
                builder.Append(String.Format("Source {0}:\n{1}", i + 1, Sources[i].ToString()));
            }
            for (int i = 0; i < NumSources; i++)
            {
                builder.Append(String.Format("DCA: {0}", Amplifiers[i].ToString()));
            }

            builder.Append(String.Format("F1: {0}\n", Filter1.ToString()));
            builder.Append(String.Format("F2: {0}\n", Filter2.ToString()));
            return builder.ToString();
        }

        private byte ComputeChecksum()
        {
            byte sum = 0;
            byte[] data = CollectData();
            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
            }
            sum += 0xA5;
            return sum;
        }

        private byte[] CollectData()
        {
            List<byte> data = new List<byte>();
            
            data.AddRange(Common.ToData());

            byte[] source1Data = Sources[0].ToData();
            byte[] source2Data = Sources[1].ToData();
            byte[] source3Data = Sources[2].ToData();
            byte[] source4Data = Sources[3].ToData();

            for (int i = 0; i < Source.DataSize; i++)
            {
                data.Add(source1Data[i]);
                data.Add(source2Data[i]);
                data.Add(source3Data[i]);
                data.Add(source4Data[i]);
            }

            byte[] amp1Data = Amplifiers[0].ToData();
            byte[] amp2Data = Amplifiers[1].ToData();
            byte[] amp3Data = Amplifiers[2].ToData();
            byte[] amp4Data = Amplifiers[3].ToData();

            for (int i = 0; i < Amplifier.DataSize; i++)
            {
                data.Add(amp1Data[i]);
                data.Add(amp2Data[i]);
                data.Add(amp3Data[i]);
                data.Add(amp4Data[i]);
            }

            byte[] filter1Data = Filter1.ToData();
            byte[] filter2Data = Filter2.ToData();
            for (int i = 0; i < Filter.DataSize; i++)
            {
                data.Add(filter1Data[i]);
                data.Add(filter2Data[i]);
            }

            return data.ToArray();
        }

        public byte[] ToData()
        {
            byte sum = ComputeChecksum();
            List<byte> allData = new List<byte>();
            allData.AddRange(CollectData());
            allData.Add(sum);
            return allData.ToArray();
        }
    }
}