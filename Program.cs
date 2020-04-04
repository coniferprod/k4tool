using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CommandLine;

using KSynthLib.Common;
using KSynthLib.K4;

namespace k4tool
{
    [Verb("list", HelpText = "List contents of bank.")]
    public class ListOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'html')")]
        public string Output { get; set; }
    }

    [Verb("dump", HelpText = "Dump contents of bank.")]
    public class DumpOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }
    }

    [Verb("report", HelpText = "Report on the specified bank.")]
    public class ReportOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }
    }

    [Verb("init", HelpText = "Initialize a new bank.")]
    public class InitOptions
    {
        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName { get; set; }
    }

    class Program
    {
        public const int SinglePatchCount = 64;  // banks A, B, C and D with 16 patches each
        public const int BankCount = 4;
        public const int PatchesPerBank = 16;

        public const int MultiPatchCount = 64;   // same as single
        public const int EffectPatchCount = 32;

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<ListOptions, DumpOptions, ReportOptions, InitOptions>(args);
            parserResult.MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (ReportOptions opts) => RunReportAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
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
                byte[] singleData = new byte[SinglePatch.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, SinglePatch.DataSize);
                SinglePatch single = new SinglePatch(singleData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"S{name}  {single.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += SinglePatch.DataSize;
            }
            sb.Append("\n");

            sb.Append("MULTI patches:\n");
            for (int i = 0; i < MultiPatchCount; i++)
            {
                byte[] multiData = new byte[MultiPatch.DataSize];
                Buffer.BlockCopy(data, offset, multiData, 0, MultiPatch.DataSize);
                MultiPatch multi = new MultiPatch(multiData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"M{name}  {multi.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += MultiPatch.DataSize;
            }

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<tt><table>\n");

            SinglePatch[][] banks = new SinglePatch[BankCount][]; //BankCount, PatchesPerBank];

            int offset = 0;

            for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                SinglePatch[] patches = new SinglePatch[PatchesPerBank];
                for (int patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    byte[] singleData = new byte[SinglePatch.DataSize];
                    Buffer.BlockCopy(data, offset, singleData, 0, SinglePatch.DataSize);
                    SinglePatch single = new SinglePatch(singleData);
                    patches[patchNumber] = single;
                    offset += SinglePatch.DataSize;
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
                    SinglePatch[] patches = banks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    SinglePatch single = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{single.Name:10}</td>\n"));
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

            return 0;
        }

        public static int RunReportAndReturnExitCode(ReportOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                ProcessMessage(message);
            }

            return 0;
        }

        public static int RunInitAndReturnExitCode(InitOptions opts)
        {
            List<SinglePatch> singlePatches = new List<SinglePatch>();
            for (int i = 0; i < SinglePatchCount; i++)
            {
                SinglePatch single = new SinglePatch();
                singlePatches.Add(single);
            }

            List<MultiPatch> multiPatches = new List<MultiPatch>();
            for (int i = 0; i < MultiPatchCount; i++)
            {
                MultiPatch multi = new MultiPatch();
                multiPatches.Add(multi);
            }

            // Create a System Exclusive header for an "All Patch Data Dump"
            SystemExclusiveHeader header = new SystemExclusiveHeader();
            header.ManufacturerID = 0x40;  // Kawai
            header.Channel = 0;  // MIDI channel 1
            header.Function = (byte)SystemExclusiveFunction.AllPatchDataDump;
            header.Group = 0;  // synthesizer group
            header.MachineID = 0x04;  // K4/K4r ID
            header.Substatus1 = 0;  // INT
            header.Substatus2 = 0;  // always zero
            
            List<byte> data = new List<byte>();
            data.Add(SystemExclusiveHeader.Initiator);            
            data.AddRange(header.ToData());

            // Single patches: 64 * 131 = 8384 bytes of data
            foreach (SinglePatch s in singlePatches)
            {
                data.AddRange(s.ToData());
            }

            // Multi patches: 64 * 77 = 4928 bytes of data
            // The K4 MIDI spec has an error in the "All Patch Data" description;
            // multi patches are listed as 87, not 77 bytes
            foreach (MultiPatch m in multiPatches)
            {
                data.AddRange(m.ToData());
            }

            // Drums: 682 bytes of data
            DrumPatch drums = new DrumPatch();
            data.AddRange(drums.ToData());

            List<EffectPatch> effectPatches = new List<EffectPatch>();
            for (int i = 0; i < EffectPatchCount; i++)
            {
                EffectPatch effect = new EffectPatch();
                effectPatches.Add(effect);
            }

            // Effect patches: 32 * 35 = 1120 bytes of data
            foreach (EffectPatch e in effectPatches)
            {
                data.AddRange(e.ToData());
            }

            data.Add(SystemExclusiveHeader.Terminator);

            // SysEx initiator:    1 
            // SysEx header:       7
            // Single patches:  8384
            // Multi patches:   4928
            // Drum settings:    682
            // Effect patches:  1120
            // SysEx terminator:   1
            // Total bytes:    15123

            // Write the data to the output file
            File.WriteAllBytes(opts.OutputFileName, data.ToArray());

            return 0;
        }

        private static void ProcessMessage(byte[] message)
        {
            SystemExclusiveHeader header = new SystemExclusiveHeader(message);

            Console.WriteLine("{0}", header);

            Dictionary<SystemExclusiveFunction, string> functionNames = new Dictionary<SystemExclusiveFunction, string>()
            {
                { SystemExclusiveFunction.AllPatchDataDump, "All Patch Data Dump" },
                { SystemExclusiveFunction.AllPatchDumpRequest, "All Patch Data Dump Request" },
                { SystemExclusiveFunction.BlockPatchDataDump, "Block Patch Data Dump" },
                { SystemExclusiveFunction.BlockPatchDumpRequest, "Block Patch Data Dump Request" },
                { SystemExclusiveFunction.EditBufferDump, "Edit Buffer Dump" },
                { SystemExclusiveFunction.OnePatchDataDump, "One Patch Data Dump" },
                { SystemExclusiveFunction.OnePatchDumpRequest, "One Patch Data Dump Request" },
                { SystemExclusiveFunction.ParameterSend, "Parameter Send" },
                { SystemExclusiveFunction.ProgramChange, "Program Change" },
                { SystemExclusiveFunction.WriteComplete, "Write Complete" },
                { SystemExclusiveFunction.WriteError, "Write Error" },
                { SystemExclusiveFunction.WriteErrorNoCard, "Write Error (No Card)" },
                { SystemExclusiveFunction.WriteErrorProtect, "Write Error (Protect)" }
            };

            SystemExclusiveFunction function = (SystemExclusiveFunction)header.Function;
            string functionName = "";
            if (functionNames.TryGetValue(function, out functionName))
            {
                Console.WriteLine("Function = {0}", functionName);
            }
            else
            {
                Console.WriteLine("Unknown function: {0}", function);
            }
        }

        public static string GetNoteName(int noteNumber) {
            string[] notes = new string[] {"A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            int octave = noteNumber / 12 + 1;
            string name = notes[noteNumber % 12];
            return name + octave;
        }
    }
}
