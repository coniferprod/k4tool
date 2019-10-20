using System;
using System.Text;
using System.Collections.Generic;

namespace k4tool
{
    public class Envelope
    {
        public int Attack; // 0~100

        public int Decay; // 0~100

        public int Sustain; // 0~100

        public int Release; // 0~100

        public Envelope()
        {
            Attack = 0;
            Decay = 0;
            Sustain = 0;
            Release = 0;
        }

        public Envelope(int a, int d, int s, int r)
        {
            Attack = a;
            Decay = d;
            Sustain = s;
            Release = r;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("A:{0} D:{1} S:{2} R:{3}", Attack, Decay, Sustain, Release));
            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)Attack);
            data.Add((byte)Decay);
            data.Add((byte)Sustain);
            data.Add((byte)Release);
            return data.ToArray();
        }

    }
}