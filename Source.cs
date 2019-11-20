using System;
using System.Text;
using System.Collections.Generic;

namespace k4tool
{
    public class Source
    {
        public const int DataSize = 7;

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

        public Source()
        {
        }

        public Source(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            (b, offset) = Util.GetNextByte(data, offset);
            Delay = b & 0x7f;

            int waveSelectHigh = 0;
            int waveSelectLow = 0;
            int keyScalingCurve = 0;
            (b, offset) = Util.GetNextByte(data, offset);
            waveSelectHigh = b & 0x01;
            keyScalingCurve = ((b >> 4) & 0x07);

            byte b2 = 0;
            (b2, offset) = Util.GetNextByte(data, offset);
            waveSelectLow = b2 & 0x7f;

            // Combine the wave select bits:
            string waveSelectBitString = String.Format("{0}{1}", Convert.ToString(waveSelectHigh, 2), Convert.ToString(waveSelectLow, 2));
            WaveNumber = Convert.ToInt32(waveSelectBitString, 2) + 1;

            KeyScalingCurve = ((b >> 4) & 0x07);  // 0 ~7 / 1 ~ 8

            (b, offset) = Util.GetNextByte(data, offset);
            // Here the MIDI implementation's SysEx format is a little unclear.
            // My interpretation is that the low six bits are the coarse value,
            // and b6 is the key tracking bit (b7 is zero).
            KeyTracking = b.IsBitSet(6);
            Coarse = (b & 0x3f) - 24;  // 00 ~ 48 /±24

            (b, offset) = Util.GetNextByte(data, offset);
            FixedKey = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            Fine = b & 0x7f;

            (b, offset) = Util.GetNextByte(data, offset);
            PressureToFrequencySwitch = b.IsBitSet(0);
            VibratoSwitch = b.IsBitSet(1);
            VelocityCurve = ((b >> 2) & 0x07);
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
            return builder.ToString();
        }

        public static string GetNoteName(int noteNumber) {
            string[] notes = new string[] {"A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            int octave = noteNumber / 12 + 1;
            string name = notes[noteNumber % 12];
            return name + octave;
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)Delay);

            // Make an 8-bit binary string representation
            string waveNumberString = Convert.ToString(WaveNumber, 2).PadLeft(8, '0');
            //System.Console.WriteLine(String.Format("wave number in binary = '{0}'", waveNumberString));
            string ksCurveString = Convert.ToString(KeyScalingCurve, 2).PadLeft(3, '0');
            StringBuilder b34 = new StringBuilder("0");
            b34.Append(ksCurveString);
            b34.Append("000");
            b34.Append(waveNumberString[0]);
            data.Add(Convert.ToByte(b34.ToString(), 2));
            data.Add(Convert.ToByte(waveNumberString.Substring(1), 2));

            // Pack the coarse and key track values into one byte
            StringBuilder b42 = new StringBuilder("0");
            b42.Append(KeyTracking ? "1" : "0");
            b42.Append(Convert.ToString(Coarse + 24, 2).PadLeft(6, '0'));
            data.Add(Convert.ToByte(b42.ToString(), 2));

            data.Add((byte)FixedKey);
            data.Add((byte)Fine);

            // Pack the velocity curve and a few other values into one byte
            StringBuilder b54 = new StringBuilder();
            b54.Append(Convert.ToString(VelocityCurve, 2).PadLeft(6, '0'));
            b54.Append(VibratoSwitch ? "1" : "0");
            b54.Append(PressureToFrequencySwitch ? "1" : "0");
            data.Add(Convert.ToByte(b54.ToString(), 2));

            return data.ToArray();
        }        
    }
}
