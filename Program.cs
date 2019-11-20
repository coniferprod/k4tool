using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CommandLine;

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

    public class SystemExclusiveHeader
    {
        public const int DataSize = 8;

        public byte ManufacturerID;
	    public byte Channel;
	    public byte Function;
	    public byte Group;
	    public byte MachineID;
	    public byte Substatus1;
	    public byte Substatus2;

        public SystemExclusiveHeader(byte[] data)
        {
            // TODO: Check that data[0] is the SysEx identifier $F0
            ManufacturerID = data[1];
            Channel = data[2];
		    Function = data[3];
		    Group = data[4];
		    MachineID = data[5];
		    Substatus1 = data[6];
		    Substatus2 = data[7];
        }

        public override string ToString()
        {
            return String.Format("ManufacturerID = {0,2:X2}h, Channel = {1,2:X2}h, Function = {2,2:X2}h, Group = {3,2:X2}h, MachineID = {4,2:X2}h, Substatus1 = {5,2:X2}h, Substatus2 = {6,2:X2}h", ManufacturerID, Channel, Function, Group, MachineID, Substatus1, Substatus2);
        }
    }

    [Verb("list", HelpText = "List contents of bank.")]
    public class ListOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'html'")]
        public string Output { get; set; }
    }

    [Verb("dump", HelpText = "Dump contents of bank.")]
    public class DumpOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }
    }


    class Program
    {
        public const int SinglePatchCount = 64;  // banks A, B, C and D with 16 patches each
        public const int BankCount = 4;
        public const int PatchesPerBank = 16;

        public const int MultiPatchCount = 64;   // same as single

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<ListOptions, DumpOptions>(args);
            parserResult.MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                errs => 1
            );

            return 0;
        }

        public static int RunListAndReturnExitCode(ListOptions opts)
        {
            string fileName = opts.FileName;
            byte[] message = File.ReadAllBytes(fileName);
            string namePart = new DirectoryInfo(fileName).Name;
            DateTime timestamp = File.GetLastWriteTime(fileName);
            string timestampString = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
            Console.WriteLine($"System Exclusive file: '{namePart}' ({timestampString}, {message.Length} bytes)");

            SystemExclusiveHeader header = new SystemExclusiveHeader(message);
            // TODO: Check the SysEx file header for validity

            // Extract the patch bytes (discarding the SysEx header and terminator)
            int dataLength = message.Length - SystemExclusiveHeader.DataSize - 1;
            //System.Console.WriteLine($"data length = {dataLength}");
            byte[] data = new byte[dataLength];
            Array.Copy(message, SystemExclusiveHeader.DataSize, data, 0, dataLength);

            // TODO: Split the data into chunks representing single, multi, drum, and effect data
            //Console.WriteLine(String.Format("Total data length = {0} bytes", data.Length));

            string outputFormat = opts.Output;
            if (outputFormat.Equals("text"))
            {
                Console.WriteLine(MakeTextList(data, namePart));
                return 0;
            }
            else if (outputFormat.Equals("html"))
            {
                Console.WriteLine(MakeHtmlList(data, namePart));
                return 0;
            }
            else 
            {
                Console.WriteLine(String.Format($"Unknown output format: '{outputFormat}'"));
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

            int offset = 0;
            for (int i = 0; i < SinglePatchCount; i++)
            {
                byte[] singleData = new byte[Single.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, Single.DataSize);
                Single single = new Single(singleData);
                string name = GetPatchName(i);
                sb.Append($"S{name}  {single.Common.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += Single.DataSize;
            }
            sb.Append("\n");

            sb.Append("MULTI patches:\n");
            for (int i = 0; i < MultiPatchCount; i++)
            {
                byte[] multiData = new byte[Multi.DataSize];
                Buffer.BlockCopy(data, offset, multiData, 0, Multi.DataSize);
                Multi multi = new Multi(multiData);
                string name = GetPatchName(i);
                sb.Append($"M{name}  {multi.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += Multi.DataSize;
            }

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<tt><table>\n");

            Single[][] banks = new Single[BankCount][]; //BankCount, PatchesPerBank];

            int offset = 0;

            for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                Single[] patches = new Single[PatchesPerBank];
                for (int patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    byte[] singleData = new byte[Single.DataSize];
                    Buffer.BlockCopy(data, offset, singleData, 0, Single.DataSize);
                    Single single = new Single(singleData);
                    patches[patchNumber] = single;
                    offset += Single.DataSize;
                }

                banks[bankNumber] = patches;
            }

            // now we should have all the patches collected in four lists of 16 each

            sb.Append("<table>\n");
            sb.Append(String.Format("<caption>{0}</caption>\n", title));
            sb.Append("<tr>\n    <th>SINGLE</th>\n");            
            for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                char bankLetter = "ABCD"[bankNumber];
                sb.Append(String.Format("    <th>{0}</th>\n", bankLetter));
            }
            sb.Append("</tr>\n");

            for (int patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
            {
                sb.Append("<tr>\n");
                sb.Append(String.Format("    <td>{0,2}</td>\n", patchNumber + 1));
                for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
                {
                    Single[] patches = banks[bankNumber];
                    string patchId = GetPatchName(bankNumber * patchNumber);
                    Single single = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{single.Common.Name:8}</td>\n"));
                }
                sb.Append("</tr>\n");
            }

            // TODO: Add multi patches

            sb.Append("</table></tt>\n");

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");
            //ProcessMessage(fileData, "list");

            return 0;
        }

        private static void ProcessMessage(byte[] message, string command)
        {
            SystemExclusiveHeader header = new SystemExclusiveHeader(message);
            // TODO: Check the SysEx file header for validity

            // Extract the patch bytes (discarding the SysEx header and terminator)
            int dataLength = message.Length - SystemExclusiveHeader.DataSize - 1;
            System.Console.WriteLine($"data length = {dataLength}");
            byte[] data = new byte[dataLength];
            Array.Copy(message, SystemExclusiveHeader.DataSize, data, 0, dataLength);

            SystemExclusiveFunction function = (SystemExclusiveFunction)header.Function;
            if (function != SystemExclusiveFunction.AllPatchDataDump)
            {
                System.Console.WriteLine($"This is not an all patch data dump: {header.ToString()}");
                // See section 5-11 in the Kawai K4 MIDI implementation manual

                return;
            }

            // TODO: Split the data into chunks representing single, multi, drum, and effect data
            Console.WriteLine(String.Format("Total data length = {0} bytes", data.Length));

            int offset = 0;

            Console.WriteLine(String.Format("Single patches (starting at offset {0}):", offset));
            for (int i = 0; i < SinglePatchCount; i++)
            {
                Console.WriteLine(String.Format("offset = {0}:", offset));
                byte[] singleData = new byte[Single.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, Single.DataSize);
                Single single = new Single(singleData);
                string name = GetPatchName(i);
                System.Console.WriteLine($"S{name} {single.Common.Name}");
                //System.Console.WriteLine(single.ToString());
                //System.Console.WriteLine();
                offset += Single.DataSize;
            }

            Console.WriteLine(String.Format("Multi patches (starting at offset {0}):", offset));
            for (int i = 0; i < MultiPatchCount; i++)
            {
                Console.WriteLine(String.Format("offset = {0}:", offset));
                byte[] multiData = new byte[Multi.DataSize];
                Buffer.BlockCopy(data, offset, multiData, 0, Multi.DataSize);
                Multi multi = new Multi(multiData);
                string name = GetPatchName(i);
                System.Console.WriteLine($"M{name} {multi.Name}");
                //System.Console.WriteLine(multi.ToString());
                //System.Console.WriteLine();
                offset += Multi.DataSize;
            }
        }

        public static string GetPatchName(int p, int patchCount = 16)
        {
        	int bankIndex = p / patchCount;
	        char bankLetter = "ABCD"[bankIndex];
	        int patchIndex = (p % patchCount) + 1;

	        return String.Format("{0}-{1,2}", bankLetter, patchIndex);
        }

        public static string GetNoteName(int noteNumber) {
            string[] notes = new string[] {"A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            int octave = noteNumber / 12 + 1;
            string name = notes[noteNumber % 12];
            return name + octave;
        }
    }
}
