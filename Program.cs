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
                InitOptions,
                GenerateOptions,
                ExtractOptions,
                InjectOptions,
                WaveOptions>(args);
            parserResult.MapResult(
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                (ExtractOptions opts) => RunExtractAndReturnExitCode(opts),
                (InjectOptions opts) => RunInjectAndReturnExitCode(opts),
                (WaveOptions opts) => RunWaveAndReturnExitCode(opts),
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

        private static string MakeSinglePatchText(SinglePatch singlePatch)
        {
            string MakeSingleColumnRow(string label, string value)
            {
                return String.Format($"{label,-10}{value}");
            }

            string MakeTwoColumnRow(string category, string label, string value, bool isFirst = false)
            {
                StringBuilder sb = new StringBuilder();

                string space = " ";
                if (isFirst)
                {
                    sb.Append(String.Format($"{category,-10}"));
                }
                else
                {
                    sb.Append(String.Format($"{space,-10}"));
                }

                sb.Append(String.Format($"{label,-20} {value}"));

                return sb.ToString();
            }

            string CenteredString(string s, int desiredLength)
            {
                if (s.Length >= desiredLength)
                {
                    return s;
                }
                int firstPad = (s.Length + desiredLength) / 2;
                return s.PadLeft(firstPad).PadRight(desiredLength);
            }

            List<string> lines = new List<string>();

            lines.Add(MakeSingleColumnRow("Volume", singlePatch.Volume.ToString()));
            lines.Add(MakeSingleColumnRow("Effect", singlePatch.Effect.ToString()));
            lines.Add(MakeSingleColumnRow("Submix ch", singlePatch.Submix.ToString()));
            lines.Add(MakeSingleColumnRow("Name", singlePatch.Name));
            lines.Add(MakeTwoColumnRow("Common", "Source Mode", singlePatch.SourceMode.ToString(), true));

            StringBuilder amValue = new StringBuilder();
            if (singlePatch.AM12)
            {
                amValue.Append("1>2");
            }
            if (singlePatch.AM34)
            {
                amValue.Append(" 3>4");
            }
            lines.Add(MakeTwoColumnRow("Common", "AM", amValue.ToString()));

            lines.Add(MakeTwoColumnRow("Common", "Poly Mode", singlePatch.PolyphonyMode.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "Bender Range", singlePatch.PitchBendRange.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "Press Freq", singlePatch.PressureFreq.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "Wheel Assign", singlePatch.WheelAssign.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "      Depth", singlePatch.WheelDepth.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "Auto Bend Time", singlePatch.AutoBend.Time.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "          Depth", singlePatch.AutoBend.Depth.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "          KS Time", singlePatch.AutoBend.KeyScalingTime.ToString()));
            lines.Add(MakeTwoColumnRow("Common", "          Vel Depth", singlePatch.AutoBend.VelocityDepth.ToString()));

            lines.Add(MakeTwoColumnRow("LFO", "Vibrato Shape", singlePatch.Vibrato.Shape.ToString(), true));
            lines.Add(MakeTwoColumnRow("LFO", "        Speed", singlePatch.Vibrato.Speed.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Depth", singlePatch.Vibrato.Depth.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Press Depth", singlePatch.Vibrato.Pressure.ToString()));

            lines.Add(MakeTwoColumnRow("LFO", "DCF-LFO Shape", singlePatch.LFO.Shape.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Speed", singlePatch.LFO.Speed.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Delay", singlePatch.LFO.Delay.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Depth", singlePatch.LFO.Depth.ToString()));
            lines.Add(MakeTwoColumnRow("LFO", "        Press Depth", singlePatch.LFO.PressureDepth.ToString()));

            string space = " ";
            lines.Add(String.Format("{0,-30}{1}{2}{3}{4}", space, CenteredString("S1", 10), CenteredString("S2", 10), CenteredString("S3", 10), CenteredString("S4", 10)));

            StringBuilder sourceValues = new StringBuilder();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Delay.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "Delay", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.VelocityCurve.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "Vel curve", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.KeyScalingCurve.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "KS curve", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.WaveNumber.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Wave", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.KeyTrack.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Key Track", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Coarse.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Coarse", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Fine.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Fine", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.PressureFrequency.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Press freq", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Source source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Vibrato.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Vib/A.bend", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.EnvelopeLevel.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Level", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Attack.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Attack", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Decay.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Decay", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Sustain.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Sustain", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Release.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Release", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.VelocityDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Vel Depth", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.PressureDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Press Depth", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.KeyScalingDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "KS Depth", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.AttackVelocity.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Time Mod Attack", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.ReleaseVelocity.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "         Release", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (Amplifier amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.KeyScaling.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "         KS", sourceValues.ToString(), false));

            lines.Add(String.Format("{0,-30}{1}{2}", space, CenteredString("F1", 20), CenteredString("F2", 20)));

            lines.Add(MakeTwoColumnRow("DCF", "Cutoff",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Cutoff.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Cutoff.ToString(), 20)), true));

            lines.Add(MakeTwoColumnRow("DCF", "Resonance",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Resonance.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Resonance.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "Vel Depth",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.CutoffMod.VelocityDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.CutoffMod.VelocityDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "KS Depth",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.CutoffMod.KeyScalingDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.CutoffMod.KeyScalingDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "LFO",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.IsLFO.ToString(), 20),
                    CenteredString(singlePatch.Filter2.IsLFO.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Env Depth",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.EnvelopeDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.EnvelopeDepth.ToString(), 20)), true));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Vel Depth",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.EnvelopeVelocityDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.EnvelopeVelocityDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Attack",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Attack.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Attack.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Decay",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Decay.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Decay.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Sustain",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Sustain.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Sustain.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Release",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Release.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Release.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Time Mod Attack",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.AttackVelocity.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.AttackVelocity.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "         Release",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.ReleaseVelocity.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.ReleaseVelocity.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "         KS",
                String.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.KeyScaling.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.KeyScaling.ToString(), 20))));

            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
                sb.Append("\n");
            }
            return sb.ToString();
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            //Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            Bank bank = new Bank(fileData);
            //Console.WriteLine($"Bank: {bank.Singles.Count} single patches, {bank.Multis.Count} multi patches");

            string outputFormat = opts.Output;
            if (outputFormat.Equals("text"))
            {
                Console.WriteLine("Single patches:");
                int patchNumber = 0;
                foreach (SinglePatch sp in bank.Singles)
                {
                    string patchId = PatchUtil.GetPatchName(patchNumber);
                    Console.WriteLine(patchId);
                    Console.WriteLine(MakeSinglePatchText(sp));
                    patchNumber++;
                }

/*
                patchNumber = 0;
                Console.WriteLine("Multi patches:");
                foreach (MultiPatch mp in bank.Multis)
                {
                    string patchId = PatchUtil.GetPatchName(patchNumber);
                    Console.WriteLine(patchId);
                    Console.WriteLine(mp.ToString());
                    patchNumber++;
                }
*/
                Console.WriteLine("Drum and effect patches: later");

                return 0;
            }
            else if (outputFormat.Equals("json"))
            {
                string json = JsonConvert.SerializeObject(
                    bank,
                    Formatting.Indented,
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

        public static int RunWaveAndReturnExitCode(WaveOptions opts)
        {
            for (int i = 1; i <= Wave.WaveCount; i++)
            {
                Console.WriteLine($"{i,3} {Wave.Names[i]}");
            }

            return 0;
        }

        public static string GetNoteName(int noteNumber) {
            string[] notes = new string[] {"A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            int octave = noteNumber / 12 + 1;
            string name = notes[noteNumber % 12];
            return name + octave;
        }

        public static int GetPatchNumber(string s)
        {
            string us = s.ToUpper();
            char[] bankNames = new char[] { 'A', 'B', 'C', 'D' };
            int bankIndex = Array.IndexOf(bankNames, us[0]);
            if (bankIndex < 0)
            {
                return 0;
            }

            int number = 0;
            string ns = us.Substring(1);  // take the rest after the bank letter
            try
            {
                number = Int32.Parse(ns) - 1;  // bring to range 0...15
            }
            catch (FormatException)
            {
                Console.WriteLine($"bad patch number: '{s}'");
            }

            return bankIndex * 16 + number;
        }
    }
}
