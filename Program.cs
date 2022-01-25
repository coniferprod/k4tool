using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using CommandLine;

using KSynthLib.Common;
using KSynthLib.K4;

using Newtonsoft.Json;

namespace K4Tool
{
    class Program
    {
        public const int BankCount = 4;
        public const int PatchesPerBank = 16;

        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<
                ListOptions,
                DumpOptions,
                InitOptions,
                GenerateOptions,
                ExtractOptions,
                InjectOptions,
                CopyOptions,
                WaveOptions,
                TableOptions,
                InfoOptions>(args);
            parserResult.MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                (ExtractOptions opts) => RunExtractAndReturnExitCode(opts),
                (InjectOptions opts) => RunInjectAndReturnExitCode(opts),
                (CopyOptions opts) => RunCopyAndReturnExitCode(opts),
                (WaveOptions opts) => RunWaveAndReturnExitCode(opts),
                (TableOptions opts) => RunTableAndReturnExitCode(opts),
                (InfoOptions opts) => RunInfoAndReturnExitCode(opts),
                errs => 1
            );

            return 0;
        }

        public static int RunListAndReturnExitCode(ListOptions opts)
        {
            Console.SetError(new StreamWriter(@"errors.txt"));

            string fileName = opts.FileName;
            byte[] message = File.ReadAllBytes(fileName);
            string namePart = new DirectoryInfo(fileName).Name;
            DateTime timestamp = File.GetLastWriteTime(fileName);
            var timestampString = timestamp.ToString("yyyy-MM-dd hh:mm:ss");
            Console.WriteLine($"System Exclusive file: '{namePart}' ({timestampString}, {message.Length} bytes)");

            var header = new SystemExclusiveHeader(message);
            // TODO: Check the SysEx file header for validity

            // Extract the patch bytes (discarding the SysEx header and terminator)
            var dataLength = message.Length - SystemExclusiveHeader.DataSize - 1;
            var data = new byte[dataLength];
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

            var sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

            var offset = 0;
            for (var i = 0; i < Bank.SinglePatchCount; i++)
            {
                var singleData = new byte[SinglePatch.DataSize];
                Buffer.BlockCopy(data, offset, singleData, 0, SinglePatch.DataSize);
                //Console.WriteLine($"Constructing single patch from {singleData.Length} bytes of data starting at {offset}");
                var singlePatch = new SinglePatch(singleData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"S{name}  {singlePatch.Name}\n");
                if ((i + 1) % 16 == 0)
                {
                    sb.Append("\n");
                }
                offset += SinglePatch.DataSize;
            }
            sb.Append("\n");

            sb.Append("MULTI patches:\n");
            for (var i = 0; i < Bank.MultiPatchCount; i++)
            {
                var multiData = new byte[MultiPatch.DataSize];
                Buffer.BlockCopy(data, offset, multiData, 0, MultiPatch.DataSize);
                //Console.WriteLine($"Constructing multi patch from {multiData.Length} bytes of data starting at {offset}");
                var multiPatch = new MultiPatch(multiData);
                string name = PatchUtil.GetPatchName(i);
                sb.Append($"M{name}  {multiPatch.Name}\n");
                if ((i + 1) % 16 == 0)
                {
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
            for (var i = 0; i < Bank.EffectPatchCount; i++)
            {
                var effectData = new byte[EffectPatch.DataSize];
                Buffer.BlockCopy(data, offset, effectData, 0, EffectPatch.DataSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                var effectPatch = new EffectPatch(effectData);
                sb.Append($"E-{i+1,2}  {effectPatch}");
                offset += EffectPatch.DataSize;
            }

            return sb.ToString();
        }

        private static string MakeHtmlList(byte[] data, string title)
        {
            var sb = new StringBuilder();

            sb.Append(String.Format("<h1>{0}</h1>\n", title));

            SinglePatch[][] singleBanks = new SinglePatch[BankCount][]; //BankCount, PatchesPerBank];

            int offset = 0;
            int patchSize = SinglePatch.DataSize;

            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                SinglePatch[] patches = new SinglePatch[PatchesPerBank];
                for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    var singleData = new byte[patchSize];
                    Buffer.BlockCopy(data, offset, singleData, 0, patchSize);
                    var singlePatch = new SinglePatch(singleData);
                    patches[patchNumber] = singlePatch;
                    offset += patchSize;
                }

                singleBanks[bankNumber] = patches;
            }

            // Now we should have all the single patches collected in four lists of 16 each

            sb.Append("<table>\n");
            sb.Append("<tr>\n    <th>SINGLE</th>\n");
            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                char bankLetter = "ABCD"[bankNumber];
                sb.Append(String.Format("    <th>{0}</th>\n", bankLetter));
            }
            sb.Append("</tr>\n");

            for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
            {
                sb.Append("<tr>\n");
                sb.Append(String.Format("    <td>{0,2}</td>\n", patchNumber + 1));
                for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
                {
                    SinglePatch[] patches = singleBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    var singlePatch = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{singlePatch.Name:10}</td>\n"));
                }
                sb.Append("</tr>\n");
            }
            sb.Append("</table>\n");

            //
            // Multi patches
            //

            patchSize = MultiPatch.DataSize;

            MultiPatch[][] multiBanks = new MultiPatch[BankCount][];

            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                MultiPatch[] patches = new MultiPatch[PatchesPerBank];
                for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    var multiData = new byte[patchSize];
                    Buffer.BlockCopy(data, offset, multiData, 0, patchSize);
                    var multiPatch = new MultiPatch(multiData);
                    patches[patchNumber] = multiPatch;
                    offset += patchSize;
                }

                multiBanks[bankNumber] = patches;
            }

            sb.Append("<table>\n");
            sb.Append("<tr>\n    <th>MULTI</th>\n");
            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                char bankLetter = "ABCD"[bankNumber];
                sb.Append(String.Format("    <th>{0}</th>\n", bankLetter));
            }
            sb.Append("</tr>\n");

            for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
            {
                sb.Append("<tr>\n");
                sb.Append(String.Format("    <td>{0,2}</td>\n", patchNumber + 1));
                for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
                {
                    MultiPatch[] patches = multiBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    var multiPatch = patches[patchNumber];
                    sb.Append(String.Format($"    <td>{multiPatch.Name:10}</td>\n"));
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

            for (var i = 0; i < Bank.EffectPatchCount; i++)
            {
                var effectData = new byte[patchSize];
                Buffer.BlockCopy(data, offset, effectData, 0, patchSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                var effectPatch = new EffectPatch(effectData);
                sb.Append($"<tr><td>E-{i+1,2}</td><td>{effectPatch}</td></tr>\n");
                offset += patchSize;
            }

            sb.Append("</table>\n");

            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            Console.SetError(new StreamWriter(@"errors.txt"));

            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            //Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            var header = new SystemExclusiveHeader(fileData);
            // TODO: Check the SysEx file header for validity

            // Extract the patch bytes (discarding the SysEx header and terminator)
            var dataLength = fileData.Length - SystemExclusiveHeader.DataSize - 1;
            var data = new byte[dataLength];
            Array.Copy(fileData, SystemExclusiveHeader.DataSize, data, 0, dataLength);

            Bank bank = new Bank(data);
            Dump dump = new Dump(bank);

            string outputFormat = opts.Output;
            if (outputFormat.Equals("text"))
            {
                Console.WriteLine("Single patches:");
                var patchNumber = 0;
                foreach (SinglePatch sp in bank.Singles)
                {
                    string patchId = PatchUtil.GetPatchName(patchNumber).Replace(" ", String.Empty);
                    Console.WriteLine(String.Format($"SINGLE {patchId}"));
                    Console.WriteLine(dump.MakeSinglePatchText(sp));
                    patchNumber++;
                }

                patchNumber = 0;
                Console.WriteLine("Multi patches:");
                foreach (MultiPatch mp in bank.Multis)
                {
                    string patchId = PatchUtil.GetPatchName(patchNumber).Replace(" ", String.Empty);
                    Console.WriteLine(String.Format($"MULTI {patchId}"));
                    Console.WriteLine(dump.MakeMultiPatchText(mp));
                    patchNumber++;
                }

                Console.WriteLine("Drum:");
                foreach (DrumNote note in bank.Drum.Notes)
                {
                    Console.WriteLine($"S1 = {note.Source1}  S2 = {note.Source2}");
                }

                return 0;
            }
            else if (outputFormat.Equals("json"))
            {
                string json = JsonConvert.SerializeObject(
                    bank,
                    Newtonsoft.Json.Formatting.Indented,
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                );
                Console.WriteLine(json);
                return 0;
            }
            else
            {
                Console.WriteLine($"Unknown output format: '{outputFormat}'");
                return -1;
            }
        }

        public static int RunInitAndReturnExitCode(InitOptions opts)
        {
            var singlePatches = new List<SinglePatch>();
            for (var i = 0; i < Bank.SinglePatchCount; i++)
            {
                singlePatches.Add(new SinglePatch());
            }

            var multiPatches = new List<MultiPatch>();
            for (var i = 0; i < Bank.MultiPatchCount; i++)
            {
                multiPatches.Add(new MultiPatch());
            }

            // Create a System Exclusive header for an "All Patch Data Dump"
            var header = new SystemExclusiveHeader();
            header.ManufacturerID = 0x40;  // Kawai
            header.Channel = 0;  // MIDI channel 1
            header.Function = (byte)SystemExclusiveFunction.AllPatchDataDump;
            header.Group = 0;  // synthesizer group
            header.MachineID = 0x04;  // K4/K4r ID
            header.Substatus1 = 0;  // INT
            header.Substatus2 = 0;  // always zero

            var data = new List<byte>();
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

            var effectPatches = new List<EffectPatch>();
            for (var i = 0; i < Bank.EffectPatchCount; i++)
            {
                effectPatches.Add(new EffectPatch());
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

            var singlePatch = new SinglePatch();

            // Override the patch defaults
            singlePatch.Name = opts.PatchName;
            singlePatch.Volume = new LevelType(100);
            singlePatch.Effect = new EffectNumberType(1);

            byte[] data = GenerateSystemExclusiveMessage(singlePatch, PatchUtil.GetPatchNumber(opts.PatchNumber));

            Console.WriteLine("Generated single patch as a SysEx message:");
            Console.WriteLine(Util.HexDump(data));

            // Write the data to the output file
            File.WriteAllBytes(opts.OutputFileName, data);

            return 0;
        }

        private static byte[] GenerateSystemExclusiveMessage(SinglePatch patch, int patchNumber, int channel = 0)
        {
            var data = new List<byte>();

            var header = new SystemExclusiveHeader();
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

            var bank = new Bank(fileData);
            var sourcePatchNumber = PatchUtil.GetPatchNumber(opts.SourcePatchNumber);

            SinglePatch patch = bank.Singles[sourcePatchNumber];

            var destinationPatchNumber = PatchUtil.GetPatchNumber(opts.DestinationPatchNumber);
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

            Console.WriteLine("Not implemented yet");

            return 0;
        }

        private static int GetPatchOffset(string patchType, int patchNumber)
        {
            var offset = 8;  // past the SysEx header
            if (patchType.Equals("single"))
            {
                offset += patchNumber * 131;
            }
            else if (patchType.Equals("multi"))
            {
                offset += 64 * 131;  // past the single patches
                offset += patchNumber * 77;
            }
            else if (patchType.Equals("drum"))
            {
                offset += 64 * 131;  // past the single patches
                offset += 64 * 77;   // past the multi patches
            }
            else if (patchType.Equals("effect"))
            {
                offset += 64 * 131;  // past the single patches
                offset += 64 * 77;   // past the multi patches
                offset += 682;  // past drum
                offset += patchNumber * 35;
            }

            return offset;
        }

        private static int GetPatchLength(string patchType)
        {
            if (patchType.Equals("single"))
            {
                return 131;
            }
            else if (patchType.Equals("multi"))
            {
                return 77;
            }
            else if (patchType.Equals("drum"))
            {
                return 682;
            }
            else if (patchType.Equals("effect"))
            {
                return 35;
            }

            return 0;
        }

        public static List<byte> GetPatchData(List<byte> data, string patchType, int patchNumber)
        {
            var offset = GetPatchOffset(patchType, patchNumber);
            var length = GetPatchLength(patchType);
            return data.GetRange(offset, length);
        }

        public static int RunCopyAndReturnExitCode(CopyOptions options)
        {
            Console.WriteLine(options.InputFileName);
            Console.WriteLine(options.PatchType);
            Console.WriteLine(options.SourcePatchNumber);
            Console.WriteLine(options.OutputFileName);
            Console.WriteLine(options.DestinationPatchNumber);
            Console.WriteLine(options.EffectNumber);

            string inputFileName = options.InputFileName;
            byte[] inputBytes = File.ReadAllBytes(inputFileName);

            //var bank = new Bank(inputBytes);
            // Don't parse the bank, as there is a crasher in single patch parsing.
            // Just shuffle the raw bytes around for now.

            var sourcePatchNumber = PatchUtil.GetPatchNumber(options.SourcePatchNumber);
            var sourceOffset = GetPatchOffset(options.PatchType, sourcePatchNumber);
            var destinationOffset = GetPatchOffset(options.PatchType, PatchUtil.GetPatchNumber(options.DestinationPatchNumber));

            var outputData = new List<byte>(inputBytes);  // start with an exact copy of the input
            var inputData = new List<byte>(inputBytes);   // used to read the patch to be copied

            // Get the actual patch data so that we can examine it
            var patchData = GetPatchData(inputData, options.PatchType, sourcePatchNumber);

            // First insert the new data at the correct location
            int patchLength = GetPatchLength(options.PatchType);
            Console.WriteLine($"Inserting {patchLength} bytes into output data at offset {destinationOffset}");
            outputData.InsertRange(destinationOffset, inputData.GetRange(sourceOffset, patchLength));

            // Then remove the same number of bytes after it
            int removeOffset = destinationOffset + patchLength;
            Console.WriteLine($"Removing {patchLength} bytes from output data at offset {removeOffset}");
            outputData.RemoveRange(removeOffset, patchLength);

            // EffectNumber is zero if not specified
            if (options.EffectNumber != 0)
            {
                // OK, we want to copy the effect settings from the original bank to the indicated slot.

                // First get the effect settings of the original patch

                var sourceEffectNumber = 0;
                if (options.PatchType.Equals("single"))
                {
                    // Don't parse, as there is a crasher in single patch parsing.
                    // Just take the effect patch number from offset 11.
                    //var singlePatch = new SinglePatch(patchData.ToArray());
                    //sourceEffectNumber = singlePatch.Effect.Value;  // 1...32
                    sourceEffectNumber = patchData[11];
                }

                //EffectPatch effectPatch = bank.Effects[sourceEffectNumber];
                Console.WriteLine($"Source patch is using effect patch #{sourceEffectNumber}");

                var sourceEffectOffset = GetPatchOffset(options.PatchType, sourceEffectNumber - 1);  // adjusted to get correct offset
                var effectLength = GetPatchLength("effect");

                // Fetch the original effect bytes
                Console.WriteLine($"Fetching {effectLength} bytes of effect data from input data at offset {sourceEffectOffset}");
                var effectData = new List<byte>(inputData.GetRange(sourceEffectOffset, effectLength));

                // Insert them into the output data at the correct slot
                // Need to adjust the effect number to get the correct offset in the bank.
                var destinationEffectOffset = GetPatchOffset("effect", options.EffectNumber - 1);
                Console.WriteLine($"Inserting {effectLength} bytes of new effect data to output data at offset {destinationEffectOffset}");
                outputData.InsertRange(destinationEffectOffset, effectData);

                var effectRemoveOffset = destinationEffectOffset + effectLength;
                Console.WriteLine($"Removing {effectLength} bytes of old effect data from output data at offset {effectRemoveOffset}");
                outputData.RemoveRange(effectRemoveOffset, effectLength);

                // Calculate new checksum
                var payload = outputData.GetRange(8, outputData.Count - 8 - 2);  // header, old checksum and SysEx terminator
                byte checksum = GetChecksum(payload);
                Console.WriteLine("New checksum = 0x{0:X2}", checksum);
                outputData[outputData.Count - 2] = checksum;
            }

            string outputFileName = options.OutputFileName;
            File.WriteAllBytes(outputFileName, outputData.ToArray());

            return 0;
        }

        private static byte GetChecksum(List<byte> payload)
        {
            byte[] data = payload.ToArray();
            int sum = 0;
            foreach (byte b in data)
            {
                sum = (sum + b) & 0xff;
            }
            sum += 0xA5;
            return (byte)(sum & 0x7f);
        }

        private static void ProcessMessage(byte[] message)
        {
            var header = new SystemExclusiveHeader(message);
            var function = (SystemExclusiveFunction)header.Function;

            // Check the file size
            Console.WriteLine("File size: {0} bytes", message.Length);
            var systemExclusiveByteCount = SystemExclusiveHeader.DataSize + 1;  // header + terminator
            var dataSize = message.Length + systemExclusiveByteCount;
            switch (function)
            {
            case SystemExclusiveFunction.AllPatchDataDump:
                if (message.Length != 15123)
                {
                    Console.WriteLine($"Not a valid All Patch Data Dump file size (should be 15123).");
                    return;
                }
                break;

            default:
                break;
            }

            if (header.ManufacturerID != Constants.ManufacturerID)
            {
                Console.WriteLine($"Not a Kawai format System Exclusive file: manufacturer ID is {header.ManufacturerID:X2}h, should be {Constants.ManufacturerID:X2}h.");
                //Console.WriteLine($"Header: {header.ToString()}");
                return;
            }

            if (header.MachineID != (byte)MachineID.K4)
            {
                Console.WriteLine("Not a K4/K4R System Exclusive file: machine ID is {0}h, should be {1}h.", header.MachineID.ToString("X2"), ((byte)MachineID.K4).ToString("X2"));
                //Console.WriteLine($"Header: {header.ToString()}");
                return;
            }

            //Console.WriteLine(header);
            Console.WriteLine("Manufacturer: Kawai ({0:X2}h)", header.ManufacturerID);
            Console.WriteLine("Machine ID: {0:X2}h", header.MachineID);

            var functionNames = new Dictionary<SystemExclusiveFunction, string>()
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

            var functionName = "";
            if (functionNames.TryGetValue(function, out functionName))
            {
                Console.WriteLine($"Function: {functionName}");
            }
            else
            {
                Console.WriteLine($"Unknown function: {function}");
                return;
            }

            var locality = "I";
            var patchName = "";
            switch (function)
            {
            case SystemExclusiveFunction.OnePatchDataDump:
                switch (header.Substatus1)
                {
                case 0x00:
                    locality = "I";
                    //Console.WriteLine("INTERNAL");
                    patchName = PatchUtil.GetPatchName(header.Substatus2);
                    if (header.Substatus2 < 64)
                    {
                        Console.WriteLine($"SINGLE {locality}{patchName}");
                    }
                    else
                    {
                        Console.WriteLine($"MULTI {locality}{patchName}");
                    }
                    Console.WriteLine();
                    break;

                case 0x01:
                    //Console.WriteLine("INTERNAL");
                    locality = "I";
                    patchName = PatchUtil.GetPatchName(header.Substatus2);
                    if (header.Substatus2 < 32)
                    {
                        Console.WriteLine($"EFFECT {locality}{patchName}");
                    }
                    else
                    {
                        Console.WriteLine($"DRUM {locality}");
                    }
                    break;

                case 0x02:
                    //Console.WriteLine("EXTERNAL");
                    locality = "E";
                    patchName = PatchUtil.GetPatchName(header.Substatus2);
                    if (header.Substatus2 < 64)
                    {
                        Console.WriteLine($"SINGLE {locality}{patchName}");
                    }
                    else
                    {
                        Console.WriteLine($"MULTI {locality}{patchName}");
                    }
                    break;

                case 0x03:
                    //Console.WriteLine("EXTERNAL");
                    locality = "E";
                    patchName = PatchUtil.GetPatchName(header.Substatus2);
                    if (header.Substatus2 < 32)
                    {
                        Console.WriteLine($"EFFECT {locality}{patchName}");
                    }
                    else
                    {
                        Console.WriteLine($"DRUM {locality}");
                    }
                    break;

                default:
                    Console.WriteLine("Unknown");
                    break;
                }
                break;

            case SystemExclusiveFunction.BlockPatchDataDump:
                switch (header.Substatus1)
                {
                case 0x00:
                case 0x01:
                    Console.WriteLine("INTERNAL");
                    break;

                case 0x02:
                case 0x03:
                    Console.WriteLine("EXTERNAL");
                    break;

                default:
                    Console.WriteLine("Not INT or EXT");
                    break;
                }

                switch (header.Substatus2)
                {
                case 0x00:
                    if (header.Substatus1 == 0x01 || header.Substatus1 == 0x03)
                    {
                        Console.WriteLine("All EFFECTs");
                    }
                    else
                    {
                        Console.WriteLine("All SINGLEs");
                    }
                    break;

                case 0x40:
                    Console.WriteLine("All MULTIs");
                    break;

                default:
                    Console.WriteLine("Not SINGLEs or MULTIs");
                    break;
                }
                break;

            case SystemExclusiveFunction.AllPatchDataDump:
                switch (header.Substatus1)
                {
                case 0x00:
                    Console.WriteLine("INTERNAL");
                    break;

                case 0x02:
                    Console.WriteLine("EXTERNAL");
                    break;

                default:
                    Console.WriteLine("Not INT or EXT");
                    break;
                }

                // NOTE: header.Substatus2 should be 0x00 at this point
                break;

            default:
                Console.WriteLine("Something else than One, Block, or All Patch Data Dump");
                break;
            }
        }

        public static int RunWaveAndReturnExitCode(WaveOptions opts)
        {
            for (var i = 1; i <= Wave.WaveCount; i++)
            {
                Console.WriteLine($"{i,3} {Wave.Names[i]}");
            }

            return 0;
        }

        public static int RunTableAndReturnExitCode(TableOptions opts)
        {
            void WriteTwoColumnParameter(XmlWriter writer, string label, string value)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c1");
                writer.WriteAttributeString("nameend", "c2");
                writer.WriteValue(label);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c3");
                writer.WriteAttributeString("nameend", "c6");
                writer.WriteAttributeString("align", "center");
                writer.WriteValue(value);
                writer.WriteEndElement(); // entry

                writer.WriteEndElement(); // row
            }

            void WriteThreeColumnParameter(XmlWriter writer, string label1, string label2, string value)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteValue(label1);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteValue(label2);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c3");
                writer.WriteAttributeString("nameend", "c6");
                writer.WriteAttributeString("align", "center");
                writer.WriteValue(value);
                writer.WriteEndElement(); // entry

                writer.WriteEndElement(); // row
            }

            void WriteSourceHeadings(XmlWriter writer)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c1");
                writer.WriteAttributeString("nameend", "c2");
                writer.WriteValue("");
                writer.WriteEndElement(); // entry

                for (int i = 1; i <= 4; i++)
                {
                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("align", "center");
                    writer.WriteValue(string.Format($"S{i}"));
                    writer.WriteEndElement(); // entry
                }

                writer.WriteEndElement(); // row
            }

            void WriteManyParameters(XmlWriter writer, string label1, string label2, List<string> values)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteValue(label1);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteValue(label2);
                writer.WriteEndElement(); // entry

                foreach (string value in values)
                {
                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("align", "center");
                    writer.WriteValue(value);
                    writer.WriteEndElement(); // entry
                }

                writer.WriteEndElement(); // row
            }

            void WriteManyParametersWithIndexTerms(XmlWriter writer, string label1, string label2, List<string> values, List<(string, string)> indexterms)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");

                writer.WriteValue(label1);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteValue(label2);
                writer.WriteEndElement(); // entry

                for (int i = 0; i < values.Count; i++)
                {
                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("align", "center");
                    writer.WriteStartElement("indexterm");
                    writer.WriteStartElement("primary");
                    writer.WriteValue(indexterms[i].Item1);
                    writer.WriteEndElement(); // primary
                    writer.WriteStartElement("secondary");
                    writer.WriteValue(indexterms[i].Item2);
                    writer.WriteEndElement(); // secondary
                    writer.WriteEndElement();  // indexterm

                    writer.WriteValue(values[i]);
                    writer.WriteEndElement(); // entry
                }

                writer.WriteEndElement(); // row
            }

            void WriteFilterParameters(XmlWriter writer, string label1, string label2, string value1, string value2)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteValue(label1);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteValue(label2);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c3");
                writer.WriteAttributeString("nameend", "c4");
                writer.WriteAttributeString("align", "center");
                writer.WriteValue(value1);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c5");
                writer.WriteAttributeString("nameend", "c6");
                writer.WriteAttributeString("align", "center");
                writer.WriteValue(value2);
                writer.WriteEndElement(); // entry

                writer.WriteEndElement(); // row
            }

            void WriteSinglePatch(XmlWriter writer, SinglePatch sp, int patchNumber)
            {
                int columnCount = SinglePatch.SourceCount + 2;

                writer.WriteStartElement("table");

                writer.WriteStartElement("indexterm");
                writer.WriteStartElement("primary");
                writer.WriteValue("single patches");
                writer.WriteEndElement(); // primary
                writer.WriteStartElement("secondary");
                writer.WriteValue(sp.Name);
                writer.WriteEndElement(); // secondary
                writer.WriteEndElement();  // indexterm

                writer.WriteStartElement("caption");
                string name = PatchUtil.GetPatchName(patchNumber).Replace(" ", "");
                writer.WriteValue(string.Format($"{name} {sp.Name}"));
                writer.WriteEndElement();

                writer.WriteStartElement("tgroup");
                writer.WriteAttributeString("cols", columnCount.ToString());

                for (var i = 1; i <= columnCount; i++)
                {
                    writer.WriteStartElement("colspec");
                    writer.WriteAttributeString("colname", string.Format($"c{i}"));
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("tbody");

                WriteTwoColumnParameter(writer, "Volume", sp.Volume.ToString());
                WriteTwoColumnParameter(writer, "Effect Patch", sp.Effect.ToString());
                WriteTwoColumnParameter(writer, "Submix Ch", sp.Submix.ToString());
                WriteTwoColumnParameter(writer, "Name", sp.Name);

                WriteThreeColumnParameter(writer, "Common", "Source Mode", sp.SourceMode.ToString());
                WriteThreeColumnParameter(writer, "", "AM", string.Format($"{sp.AM12.ToString()}, {sp.AM34.ToString()}"));
                WriteThreeColumnParameter(writer, "", "Poly Mode", sp.PolyphonyMode.ToString());
                WriteThreeColumnParameter(writer, "", "Bender Range", sp.PitchBendRange.ToString());
                WriteThreeColumnParameter(writer, "", "Press Freq", sp.PressureFreq.ToString());
                WriteThreeColumnParameter(writer, "", "Wheel Assign", sp.WheelAssign.ToString());
                WriteThreeColumnParameter(writer, "", "Wheel Depth", sp.WheelDepth.ToString());
                WriteThreeColumnParameter(writer, "", "Auto Bend Time", sp.AutoBend.Time.ToString());
                WriteThreeColumnParameter(writer, "", "Auto Bend Depth", sp.AutoBend.Depth.ToString());
                WriteThreeColumnParameter(writer, "", "Auto Bend KS Time", sp.AutoBend.KeyScalingTime.ToString());
                WriteThreeColumnParameter(writer, "", "Auto Bend Vel Depth", sp.AutoBend.VelocityDepth.ToString());

                WriteThreeColumnParameter(writer, "LFO", "Vibrato Shape", sp.Vibrato.Shape.ToString());
                WriteThreeColumnParameter(writer, "", "Vibrato Speed", sp.Vibrato.Speed.ToString());
                WriteThreeColumnParameter(writer, "", "Depth", sp.Vibrato.Depth.ToString());
                WriteThreeColumnParameter(writer, "", "Press Depth", sp.Vibrato.Pressure.ToString());
                WriteThreeColumnParameter(writer, "", "DCF-LFO Shape", sp.LFO.Shape.ToString());
                WriteThreeColumnParameter(writer, "", "DCF-LFO Speed", sp.LFO.Speed.ToString());
                WriteThreeColumnParameter(writer, "", "DCF-LFO Delay", sp.LFO.Delay.ToString());
                WriteThreeColumnParameter(writer, "", "DCF-LFO Depth", sp.LFO.Depth.ToString());
                WriteThreeColumnParameter(writer, "", "DCF-LFO Press Depth", sp.LFO.PressureDepth.ToString());

                WriteSourceHeadings(writer);

                var values = new List<string>();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.Delay.ToString());
                }
                WriteManyParameters(writer, "S-Common", "Delay", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.VelocityCurve.ToString());
                }
                WriteManyParameters(writer, "", "Vel Curve", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.KeyScalingCurve.ToString());
                }
                WriteManyParameters(writer, "", "KS Curve", values);

                var indexTerms = new List<(string, string)>();

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    string waveName = Wave.Names[source.Wave.Number];
                    values.Add(string.Format($"{source.Wave.Number} {waveName}"));
                    indexTerms.Add(("waves", waveName));
                }
                WriteManyParametersWithIndexTerms(writer, "DCO", "Wave", values, indexTerms);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.KeyTrack.ToString());
                }
                WriteManyParameters(writer, "", "Key Track", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.Coarse.ToString());
                }
                WriteManyParameters(writer, "", "Coarse", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.Fine.ToString());
                }
                WriteManyParameters(writer, "", "Fine (Fixed Key)", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.PressureFrequency.ToString());
                }
                WriteManyParameters(writer, "", "Press. Freq", values);

                values.Clear();
                foreach (Source source in sp.Sources)
                {
                    values.Add(source.Vibrato.ToString());
                }
                WriteManyParameters(writer, "", "Vib/A.Bend", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.EnvelopeLevel.ToString());
                }
                WriteManyParameters(writer, "DCA", "Level", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.Env.Attack.ToString());
                }
                WriteManyParameters(writer, "", "Attack", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.Env.Decay.ToString());
                }
                WriteManyParameters(writer, "", "Decay", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.Env.Sustain.ToString());
                }
                WriteManyParameters(writer, "", "Sustain", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.Env.Release.ToString());
                }
                WriteManyParameters(writer, "", "Release", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.LevelMod.VelocityDepth.ToString());
                }
                WriteManyParameters(writer, "DCA mod", "Vel Depth", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.LevelMod.PressureDepth.ToString());
                }
                WriteManyParameters(writer, "", "Press Depth", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.LevelMod.KeyScalingDepth.ToString());
                }
                WriteManyParameters(writer, "", "KS Depth", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.TimeMod.AttackVelocity.ToString());
                }
                WriteManyParameters(writer, "", "Time Mod Attack", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.TimeMod.ReleaseVelocity.ToString());
                }
                WriteManyParameters(writer, "", "Time Mod Release", values);

                values.Clear();
                foreach (Amplifier amplifier in sp.Amplifiers)
                {
                    values.Add(amplifier.TimeMod.KeyScaling.ToString());
                }
                WriteManyParameters(writer, "", "Time Mod KS", values);

                WriteFilterParameters(writer, "DCF", "Cutoff", sp.Filter1.Cutoff.ToString(), sp.Filter2.Cutoff.ToString());
                WriteFilterParameters(writer, "", "Resonance", sp.Filter1.Resonance.ToString(), sp.Filter2.Resonance.ToString());
                WriteFilterParameters(writer, "", "Vel Depth", sp.Filter1.CutoffMod.VelocityDepth.ToString(), sp.Filter2.CutoffMod.VelocityDepth.ToString());
                WriteFilterParameters(writer, "", "Press Depth", sp.Filter1.CutoffMod.PressureDepth.ToString(), sp.Filter2.CutoffMod.PressureDepth.ToString());
                WriteFilterParameters(writer, "", "KS Depth", sp.Filter1.CutoffMod.KeyScalingDepth.ToString(), sp.Filter2.CutoffMod.KeyScalingDepth.ToString());
                WriteFilterParameters(writer, "", "LFO", sp.Filter1.IsLFO.ToString(), sp.Filter2.IsLFO.ToString());

                WriteFilterParameters(writer, "DCF MOD", "Env Depth", sp.Filter1.EnvelopeDepth.ToString(), sp.Filter2.EnvelopeDepth.ToString());
                WriteFilterParameters(writer, "", "Vel Depth", sp.Filter1.EnvelopeVelocityDepth.ToString(), sp.Filter2.EnvelopeVelocityDepth.ToString());
                WriteFilterParameters(writer, "", "Attack", sp.Filter1.Env.Attack.ToString(), sp.Filter2.Env.Attack.ToString());
                WriteFilterParameters(writer, "", "Decay", sp.Filter1.Env.Decay.ToString(), sp.Filter2.Env.Decay.ToString());
                WriteFilterParameters(writer, "", "Sustain", sp.Filter1.Env.Sustain.ToString(), sp.Filter2.Env.Sustain.ToString());
                WriteFilterParameters(writer, "", "Release", sp.Filter1.Env.Release.ToString(), sp.Filter2.Env.Release.ToString());
                WriteFilterParameters(writer, "", "Time Mod Attack", sp.Filter1.TimeMod.AttackVelocity.ToString(), sp.Filter2.TimeMod.AttackVelocity.ToString());
                WriteFilterParameters(writer, "", "Time Mod Release", sp.Filter1.TimeMod.ReleaseVelocity.ToString(), sp.Filter2.TimeMod.ReleaseVelocity.ToString());
                WriteFilterParameters(writer, "", "Time Mod KS", sp.Filter1.TimeMod.KeyScaling.ToString(), sp.Filter2.TimeMod.KeyScaling.ToString());

                writer.WriteEndElement(); // tbody
                writer.WriteEndElement(); // tgroup
                writer.WriteEndElement(); // table
            }

            void WriteMultiTwoColumnParameter(XmlWriter writer, string label, string value)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c1");
                writer.WriteAttributeString("nameend", "c2");
                writer.WriteValue(label);
                writer.WriteEndElement(); // entry

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c3");
                writer.WriteAttributeString("nameend", "c10");
                writer.WriteAttributeString("align", "center");
                writer.WriteValue(value);
                writer.WriteEndElement(); // entry

                writer.WriteEndElement(); // row
            }

            void WriteSectionHeadings(XmlWriter writer)
            {
                writer.WriteStartElement("row");

                writer.WriteStartElement("entry");
                writer.WriteAttributeString("namest", "c1");
                writer.WriteAttributeString("nameend", "c2");
                writer.WriteValue("Section");
                writer.WriteEndElement(); // entry

                for (int i = 1; i <= MultiPatch.SectionCount; i++)
                {
                    writer.WriteStartElement("entry");
                    writer.WriteAttributeString("align", "center");
                    writer.WriteValue(i.ToString());
                    writer.WriteEndElement(); // entry
                }

                writer.WriteEndElement(); // row
            }

            void WriteMultiPatch(XmlWriter writer, MultiPatch mp, int patchNumber, List<SinglePatch> singlePatches)
            {
                var columnCount = MultiPatch.SectionCount + 2;

                writer.WriteStartElement("table");

                writer.WriteStartElement("indexterm");
                writer.WriteStartElement("primary");
                writer.WriteValue("multi patches");
                writer.WriteEndElement(); // primary
                writer.WriteStartElement("secondary");
                writer.WriteValue(mp.Name);
                writer.WriteEndElement(); // secondary
                writer.WriteEndElement();  // indexterm

                writer.WriteStartElement("caption");
                string name = PatchUtil.GetPatchName(patchNumber).Replace(" ", "");
                writer.WriteValue(string.Format($"{name} {mp.Name}"));
                writer.WriteEndElement();

                writer.WriteStartElement("tgroup");
                writer.WriteAttributeString("cols", columnCount.ToString());
                for (var i = 1; i <= columnCount; i++)
                {
                    writer.WriteStartElement("colspec");
                    writer.WriteAttributeString("colname", string.Format($"c{i}"));
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("tbody");

                WriteMultiTwoColumnParameter(writer, "Volume", mp.Volume.ToString());
                WriteMultiTwoColumnParameter(writer, "Effect Patch", mp.EffectPatch.ToString());
                WriteMultiTwoColumnParameter(writer, "Name", mp.Name);

                WriteSectionHeadings(writer);

                var values = new List<string>();
                foreach (Section section in mp.Sections)
                {
                    values.Add(PatchUtil.GetPatchName(section.SinglePatch.Value));
                }
                WriteManyParameters(writer, "Inst", "Single No.", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(singlePatches[section.SinglePatch.Value].Name);
                }
                WriteManyParameters(writer, "", "Single Name", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.KeyboardZone.Low.ToString());
                }
                WriteManyParameters(writer, "Zone", "Lo", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.KeyboardZone.High.ToString());
                }
                WriteManyParameters(writer, "", "Hi", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.VelocitySwitch.ToString());
                }
                WriteManyParameters(writer, "", "Vel Sw", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.ReceiveChannel.ToString());
                }
                WriteManyParameters(writer, "Sec Ch", "Rcv Ch", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.PlayMode.ToString());
                }
                WriteManyParameters(writer, "Sec Ch", "Mode", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.Level.ToString());
                }
                WriteManyParameters(writer, "Output", "Level", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.Transpose.ToString());
                }
                WriteManyParameters(writer, "", "Trans", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.Tune.ToString());
                }
                WriteManyParameters(writer, "", "Tune", values);

                values.Clear();
                foreach (Section section in mp.Sections)
                {
                    values.Add(section.Output.ToString());
                }
                WriteManyParameters(writer, "", "Submix Ch", values);

                writer.WriteEndElement(); // tbody
                writer.WriteEndElement(); // tgroup
                writer.WriteEndElement(); // table
            }

            string fileName = opts.InputFileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            //Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            var bank = new Bank(fileData);
            string bankName = Path.GetFileNameWithoutExtension(opts.InputFileName);

            string format = opts.Format;
            if (format.Equals("docbook"))
            {
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.NewLineOnAttributes = true;
                XmlWriter writer = XmlWriter.Create(opts.OutputFileName, settings);

                writer.WriteStartDocument();
                writer.WriteStartElement("sect1");

                writer.WriteStartElement("indexterm");
                writer.WriteStartElement("primary");
                writer.WriteValue("banks");
                writer.WriteEndElement(); // primary
                writer.WriteStartElement("secondary");
                writer.WriteValue(bankName);
                writer.WriteEndElement(); // secondary
                writer.WriteEndElement();  // indexterm

                writer.WriteStartElement("title");
                writer.WriteValue(bankName);
                writer.WriteEndElement(); // title

                writer.WriteStartElement("sect2");

                writer.WriteStartElement("title");
                writer.WriteValue("Single patches");
                writer.WriteEndElement();

                var patchNumber = 0;
                foreach (SinglePatch sp in bank.Singles)
                {
                    writer.WriteStartElement("sect3");
                    //writer.WriteAttributeString("renderas", "sect3");

                    writer.WriteStartElement("title");
                    string name = PatchUtil.GetPatchName(patchNumber).Replace(" ", "");
                    writer.WriteValue(string.Format($"{name} {sp.Name}"));
                    writer.WriteEndElement(); // title
                    WriteSinglePatch(writer, sp, patchNumber);
                    writer.WriteEndElement(); // sect3
                    writer.WriteProcessingInstruction("hard-page-break", "");
                    patchNumber++;
                }

                writer.WriteEndElement();  // sect2 for single patches

                writer.WriteStartElement("sect2");

                writer.WriteStartElement("title");
                writer.WriteValue("Multi patches");
                writer.WriteEndElement();

                patchNumber = 0;
                foreach (MultiPatch mp in bank.Multis)
                {
                    writer.WriteStartElement("sect3");
                    //writer.WriteAttributeString("renderas", "sect3");

                    writer.WriteStartElement("title");
                    string name = PatchUtil.GetPatchName(patchNumber).Replace(" ", "");
                    writer.WriteValue(string.Format($"{name} {mp.Name}"));
                    writer.WriteEndElement(); // title

                    WriteMultiPatch(writer, mp, patchNumber, bank.Singles);
                    writer.WriteEndElement(); // sect3

                    writer.WriteProcessingInstruction("hard-page-break", "");
                    patchNumber++;
                }

                writer.WriteEndElement();  // sect2 for multi patches

                writer.WriteStartElement("sect2");

                writer.WriteStartElement("title");
                writer.WriteValue("Drum");
                writer.WriteEndElement();


                writer.WriteEndElement();  // sect2 for DRUM

                writer.WriteEndDocument();
                writer.Close();
                return 0;
            }
            else if (format.Equals("html"))
            {
                Console.WriteLine("HTML output not implemented yet");
                return -1;
            }
            else
            {
                Console.WriteLine($"Unknown output format: '{format}'");
                return -1;
            }
        }

        public static int RunInfoAndReturnExitCode(InfoOptions opts)
        {
            string fileName = opts.InputFileName;
            byte[] fileData = File.ReadAllBytes(fileName);

            //SystemExclusiveHeader header = new SystemExclusiveHeader(fileData);
            ProcessMessage(fileData);
            //Console.WriteLine(header.ToString());

            return 0;
        }
    }
}
