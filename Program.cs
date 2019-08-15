using System;
using System.IO;
using System.Collections.Generic;

namespace k4tool
{
    public enum SystemExclusiveFunction
    {
        OnePatchDumpRequest = 0x00,
        BlockPatchDumpRequest = 0x01,
        AllPatchDumpRequest = 0x02,
        ParameterSend = 0x10,
        OnePatchDataDump = 0x20,
        BlockPatchDataDump = 0x21,
        AllPatchDataDump = 0x22,
        EditBufferDump = 0x23,
        ProgramChange = 0x30,
        WriteComplete = 0x40,
        WriteError = 0x41,
        WriteErrorProtect = 0x42,
        WriteErrorNoCard = 0x43
    }

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

        public const int SinglePatchCount = 64;  // banks A, B, C and D with 16 patches each
        public const int MultiPatchCount = 64;   // same as single

        public const int SingleDataSize = 131;

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
                System.Console.WriteLine($"data length = {dataLength}");
                byte[] data = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeaderLength, data, 0, dataLength);

                SystemExclusiveFunction function = (SystemExclusiveFunction)header.Function;
                if (function != SystemExclusiveFunction.AllPatchDataDump)
                {
                    System.Console.WriteLine($"This is not an all patch data dump: {header.ToString()}");
                    // See section 5-11 in the Kawai K4 MIDI implementation manual

                    break;
                }

                if (command.Equals("list"))
                {
                    // TODO: Split the data into chunks representing single, multi, drum, and effect data
                    for (int i = 0; i < SinglePatchCount; i++)
                    {
                        byte[] singleData = new byte[SingleDataSize];
                        Buffer.BlockCopy(data, i * SingleDataSize, singleData, 0, SingleDataSize);
                        Single single = new Single(singleData);
                        string name = GetPatchName(i);
                        System.Console.WriteLine($"S{name} {single.Common.Name}");
                        System.Console.WriteLine(single.ToString());
                        System.Console.WriteLine();
                    }                
                }
            }

            // For debugging: dump the wave list
            //for (int i = 0; i < Wave.NumWaves; i++)
            //{
            //    System.Console.WriteLine(String.Format("{0,3} {1}", i + 1, Wave.Instance[i]));
            //}

            return 0;
        }

        public static string GetPatchName(int p, int patchCount = 16)
        {
        	int bankIndex = p / patchCount;
	        char bankLetter = "ABCD"[bankIndex];
	        int patchIndex = (p % patchCount) + 1;

	        return String.Format("{0}-{1,2}", bankLetter, patchIndex);
        }
    }
}
