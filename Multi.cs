using System;
using System.Text;
using System.Collections.Generic;

namespace k4tool
{
    class Multi
    {
        public static readonly int SectionCount = 8;

        private string name;
        private int volume;
        private int effectPatch;

        private Section[] sections;
        private byte checksum;

        public string Name => name;

        public Multi(byte[] data)
        {
            sections = new Section[SectionCount];

            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte
            name = GetName(data, offset);
            offset += 10;  // name is M0 to M9

            (b, offset) = Util.GetNextByte(data, offset);
            volume = b;

            (b, offset) = Util.GetNextByte(data, offset);
            effectPatch = b;

            for (int i = 0; i < SectionCount; i++)
            {
                byte[] sectionData = new byte[Section.DataSize];
                Array.Copy(data, offset, sectionData, 0, Section.DataSize);
                sections[i] = new Section(sectionData);
                offset += Section.DataSize;
            }
        }

        private string GetName(byte[] data, int offset)
        {
            byte[] bytes = { data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7], data[8], data[9] };
        	return Encoding.ASCII.GetString(bytes);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("name = {0}, volume = {1}, effect = {2}\n", name, volume, effectPatch));
            for (int i = 0; i < SectionCount; i++)
            {
                builder.Append(String.Format("Section {0}:\n", i + 1));
                builder.Append(sections[i].ToString());
                builder.Append("\n");
            }

            return builder.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            return data.ToArray();
        }        
    }
}