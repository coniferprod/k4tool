using System;
using System.IO;
using System.Collections.Generic;

namespace k4tool
{
    public struct SystemExclusiveHeader
    {
        public byte ManufacturerID;
	    public byte Channel;
	    public byte Function;
	    public byte Group;
	    public byte MachineID;
	    public byte Substatus1;
	    public byte Substatus2;

        public override string ToString()
        {
            return String.Format("ManufacturerID = {0,2:X2}h, Channel = {1,2:X2}h, Function = {2,2:X2}h, Group = {3,2:X2}h, MachineID = {4,2:X2}h, Substatus1 = {5,2:X2}h, Substatus2 = {6,2:X2}h", ManufacturerID, Channel, Function, Group, MachineID, Substatus1, Substatus2);
        }
    }

    class Program
    {
        private const int SystemExclusiveHeaderLength = 8;

        static SystemExclusiveHeader GetSystemExclusiveHeader(byte[] data)
        {
            SystemExclusiveHeader header;
            // data[0] is the SysEx identifier F0H
            header.ManufacturerID = data[1];
            header.Channel = data[2];
		    header.Function = data[3];
		    header.Group = data[4];
		    header.MachineID = data[5];
		    header.Substatus1 = data[6];
		    header.Substatus2 = data[7];
            return header;
        }

        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: k4tool cmd filename.syx");
                return 1;
            }

            string command = args[0];
            string fileName = args[1];
            string patchName = "";
            if (args.Length > 2)
            {
                patchName = args[2];
            }

            byte[] fileData = File.ReadAllBytes(fileName);
            System.Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            System.Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                SystemExclusiveHeader header = GetSystemExclusiveHeader(message);
                // TODO: Check the SysEx file header for validity

                // Extract the patch bytes (discarding the SysEx header)
                int dataLength = message.Length - SystemExclusiveHeaderLength;
                byte[] data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeaderLength, data, 0, dataLength);

                Single s = new Single(data);
                
                if (header.Function != 0x22)
                {
                    System.Console.WriteLine($"This is not an all patch data dump: {header.ToString()}");
                    // See section 5-11 in the Kawai K4 MIDI implementation manual
                }
                else
                {
                    System.Console.WriteLine($"{header.ToString()}");
                }

                if (command.Equals("list"))
                {
                    System.Console.WriteLine("list");
                }
            }

            // For debugging: dump the wave list
            //for (int i = 0; i < Wave.NumWaves; i++)
            //{
            //    System.Console.WriteLine(String.Format("{0,3} {1}", i + 1, Wave.Instance[i]));
            //}

            return 0;
        }
    }
}
