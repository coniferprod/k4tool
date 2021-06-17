using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using CommandLine;

using KSynthLib.Common;
using KSynthLib.K4;

using Newtonsoft.Json;

namespace K4Tool
{
    class Program
    {
        public const int SinglePatchCount = 64;  // banks A, B, C and D with 16 patches each
        public const int BankCount = 4;
        public const int PatchesPerBank = 16;

        public const int MultiPatchCount = 64;   // same as single
        public const int EffectPatchCount = 32;

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<
                ListOptions,
                DumpOptions,
                ReportOptions,
                InitOptions,
                GenerateOptions,
                ExtractOptions,
                InjectOptions
            >(args);
            parserResult.MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (ReportOptions opts) => RunReportAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                (ExtractOptions opts) => RunExtractAndReturnExitCode(opts),
                (InjectOptions opts) => RunInjectAndReturnExitCode(opts),
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
            byte[] data = new byte[dataLength];
            Array.Copy(message, SystemExclusiveHeader.DataSize, data, 0, dataLength);

            // TODO: Split the data into chunks representing single, multi, drum, and effect data
            //Console.WriteLine($"Total data length (with/without header): {message.Length}/{data.Length} bytes");

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
                Console.WriteLine($"Unknown output format: '{outputFormat}'");
                return -1;
            }
        }

        private static string MakeTextList(byte[] data, string title)
        {
            //Console.WriteLine($"MakeTextList: data length = {data.Length} bytes");

            StringBuilder sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

            int offset = 0;
            for (int i = 0; i < SinglePatchCount; i++)
            {
                byte[] singleData = new byte[SinglePatch.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, SinglePatch.DataSize);
                //Console.WriteLine($"Constructing single patch from {singleData.Length} bytes of data starting at {offset}");
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
                //Console.WriteLine($"Constructing multi patch from {multiData.Length} bytes of data starting at {offset}");
                MultiPatch multi = new MultiPatch(multiData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"M{name}  {multi.Name}\n");
                if ((i + 1) % 16 == 0) {
                    sb.Append("\n");
                }
                offset += MultiPatch.DataSize;
            }

/*
            sb.Append("\n");
            sb.Append("DRUM:\n");
            byte[] drumData = new byte[DrumPatch.DataSize];
            Buffer.BlockCopy(data, offset, drumData, 0, DrumPatch.DataSize);
            Console.WriteLine($"Constructing drum patch from {drumData.Length} bytes of data starting at {offset}");
            DrumPatch drumPatch = new DrumPatch(drumData);
*/

            offset += DrumPatch.DataSize;

            sb.Append("\n");
            sb.Append("EFFECT SETTINGS:\n");
            for (int i = 0; i < 32; i++)
            {
                byte[] effectData = new byte[EffectPatch.DataSize];
                Buffer.BlockCopy(data, offset, effectData, 0, EffectPatch.DataSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                EffectPatch effectPatch = new EffectPatch(effectData);

                sb.Append($"E-{i+1,2}  {effectPatch}");
                offset += EffectPatch.DataSize;
            }

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            string GetNoteName(int noteNumber) {
                string[] notes = new string[] { "C", "C#", "D", "Eb", "E", "F", "F#", "G", "G#", "A", "Bb", "B" };
                int octave = noteNumber / 12 + 1;
                string name = notes[noteNumber % 12];
                return name + octave;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(String.Format("<h1>{0}</h1>\n", title));

            SinglePatch[][] singleBanks = new SinglePatch[BankCount][]; //BankCount, PatchesPerBank];

            int offset = 0;
            int patchSize = SinglePatch.DataSize;

            for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                SinglePatch[] patches = new SinglePatch[PatchesPerBank];
                for (int patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    byte[] singleData = new byte[patchSize];
                    Buffer.BlockCopy(data, offset, singleData, 0, patchSize);
                    SinglePatch single = new SinglePatch(singleData);
                    patches[patchNumber] = single;
                    offset += patchSize;
                }

                singleBanks[bankNumber] = patches;
            }

            // Now we should have all the single patches collected in four lists of 16 each

            sb.Append("<table>\n");
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
                    SinglePatch[] patches = singleBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    SinglePatch single = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{single.Name:10}</td>\n"));
                }
                sb.Append("</tr>\n");
            }
            sb.Append("</table>\n");

            //
            // Multi patches
            //

            patchSize = MultiPatch.DataSize;

            MultiPatch[][] multiBanks = new MultiPatch[BankCount][];

            for (int bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                MultiPatch[] patches = new MultiPatch[PatchesPerBank];
                for (int patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    byte[] multiData = new byte[patchSize];
                    Buffer.BlockCopy(data, offset, multiData, 0, patchSize);
                    MultiPatch multi = new MultiPatch(multiData);
                    patches[patchNumber] = multi;
                    offset += patchSize;
                }

                multiBanks[bankNumber] = patches;
            }

            sb.Append("<table>\n");
            sb.Append("<tr>\n    <th>MULTI</th>\n");
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
                    MultiPatch[] patches = multiBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    MultiPatch single = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{single.Name:10}</td>\n"));
                }
                sb.Append("</tr>\n");
            }

            sb.Append("</table>\n");

            patchSize = DrumPatch.DataSize;

            // TODO: List drum
// Crash when setting tune of drum note (value out of range)
/*
            sb.Append("<table>\n");
            sb.Append("<caption>DRUM</caption>\n");
            sb.Append("<tr><th>Note</th><th>Parameters</th></tr>\n");

            patchSize = DrumPatch.DataSize;
            byte[] drumData = new byte[patchSize];
            Buffer.BlockCopy(data, offset, drumData, 0, patchSize);
            var drum = new DrumPatch(drumData);
            for (int i = 0; i < 128; i++)
            {
                var note = drum.Notes[i];
                sb.Append($"<tr><td>E-{GetNoteName(i)}</td><td>{note}</td></tr>\n");
            }

            sb.Append("</table>\n");
*/
            offset += patchSize;

            sb.Append("<table>\n");
            sb.Append("<caption>EFFECT</caption>\n");
            sb.Append("<tr><th>#</th><th>Type and parameters</th></tr>\n");

            patchSize = EffectPatch.DataSize;

            for (int i = 0; i < 32; i++)
            {
                byte[] effectData = new byte[patchSize];
                Buffer.BlockCopy(data, offset, effectData, 0, patchSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                EffectPatch effectPatch = new EffectPatch(effectData);

                sb.Append($"<tr><td>E-{i+1,2}</td><td>{effectPatch}</td></tr>\n");
                offset += patchSize;
            }

            sb.Append("</table>\n");

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            Bank bank = new Bank(fileData);
            Console.WriteLine($"Bank: {bank.Singles.Count} single patches, {bank.Multis.Count} multi patches");
            string json = JsonConvert.SerializeObject(
                bank,
                Formatting.Indented,
                new Newtonsoft.Json.Converters.StringEnumConverter()
            );
            Console.WriteLine(json);
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

        public static int RunGenerateAndReturnExitCode(GenerateOptions opts)
        {
            if (!opts.PatchType.Equals("single"))
            {
                Console.WriteLine($"Sorry, I don't know how to generate {opts.PatchType} patches.");
                return -1;
            }

            Console.WriteLine("OK, you want to generate a single patch.");
            Console.WriteLine($"And you want to call it '{opts.PatchName}'.");

            SinglePatch singlePatch = new SinglePatch();

            // Override the patch defaults
            singlePatch.Name = opts.PatchName;
            singlePatch.Volume = 100;
            singlePatch.Effect = 1;

            byte[] data = GenerateSystemExclusiveMessage(singlePatch, PatchUtil.GetPatchNumber(opts.PatchNumber));

            Console.WriteLine("Generated single patch as a SysEx message:");
            Console.WriteLine(Util.HexDump(data));

            // Write the data to the output file
            File.WriteAllBytes(opts.OutputFileName, data);

            return 0;
        }

        private static byte[] GenerateSystemExclusiveMessage(SinglePatch patch, int patchNumber, int channel = 0)
        {
            List<byte> data = new List<byte>();

            SystemExclusiveHeader header = new SystemExclusiveHeader();
            header.ManufacturerID = 0x40;  // Kawai
            header.Channel = (byte)channel;
            header.Function = (byte)SystemExclusiveFunction.OnePatchDataDump;
            header.Group = 0x00; // synth group
            header.MachineID = 0x04; // K4/K4r
            header.Substatus1 = 0x00;  // INT
            header.Substatus2 = (byte)patchNumber;

            data.Add(SystemExclusiveHeader.Initiator);
            data.AddRange(header.ToData());
            data.AddRange(patch.ToData());
            data.Add(SystemExclusiveHeader.Terminator);

            return data.ToArray();
        }

        public static int RunExtractAndReturnExitCode(ExtractOptions opts)
        {
            if (!opts.PatchType.Equals("single"))
            {
                Console.WriteLine($"Sorry, I don't know how to extract {opts.PatchType} patches.");
                return -1;
            }

            string inputFileName = opts.InputFileName;
            byte[] fileData = File.ReadAllBytes(inputFileName);

            Bank bank = new Bank(fileData);
            int sourcePatchNumber = PatchUtil.GetPatchNumber(opts.SourcePatchNumber);

            SinglePatch patch = bank.Singles[sourcePatchNumber];

            int destinationPatchNumber = PatchUtil.GetPatchNumber(opts.DestinationPatchNumber);
            byte[] patchData = GenerateSystemExclusiveMessage(patch, destinationPatchNumber);

            // Write the data to the output file
            File.WriteAllBytes(opts.OutputFileName, patchData);

            return 0;
        }

        public static int RunInjectAndReturnExitCode(InjectOptions options)
        {
            string inputFileName = options.InputFileName;
            byte[] fileData = File.ReadAllBytes(inputFileName);

            // TODO: identify the type of the input file

            return 0;
        }

        private static void ProcessMessage(byte[] message)
        {
            SystemExclusiveHeader header = new SystemExclusiveHeader(message);

            Console.WriteLine(header);

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
                Console.WriteLine($"Function = {functionName}");
            }
            else
            {
                Console.WriteLine($"Unknown function: {function}");
            }
        }
    }
}
