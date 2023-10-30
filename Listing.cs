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
    public class Listing
    {
        public const int GroupCount = 4;
        public const int PatchesPerGroup = 16;
        public const int BankCount = 4;
        public const int PatchesPerBank = 16;

        private string Title;
        private byte[] Data;

        public Listing(string title, byte[] data)
        {
            this.Title = title;
            this.Data = data;
        }

        public string TextListing()
        {
            //Console.WriteLine($"MakeTextList: data length = {data.Length} bytes");

            var sb = new StringBuilder();
            sb.Append("SINGLE patches:\n");

            var offset = 0;
            var patchSize = SinglePatch.DataSize;

            var patchNumber = 0;

            var singlePatches = new List<SinglePatch>();
            for (patchNumber = 0; patchNumber < GroupCount * PatchesPerGroup; patchNumber++)
            {
                var singleData = new byte[patchSize];
                Buffer.BlockCopy(this.Data, offset, singleData, 0, patchSize);
                var singlePatch = new SinglePatch(singleData);
                singlePatches.Add(singlePatch);
                offset += patchSize;
            }

/*
            var patchIndex = 0;
            foreach (var singlePatch in singlePatches)
            {
                string patchLabel = PatchUtil.GetPatchName(patchIndex);
                Console.WriteLine($"S{patchLabel}  {singlePatch.Name:10}  ");
                patchIndex += 1;
            }
*/

            patchNumber = 0;
            for (var rowNumber = 0; rowNumber < PatchesPerGroup; rowNumber++)
            {
                for (var groupNumber = 0; groupNumber < GroupCount; groupNumber++)
                {
                    patchNumber = groupNumber * PatchesPerGroup + rowNumber;
                    var singlePatch = singlePatches[patchNumber];
                    string patchLabel = PatchUtil.GetPatchName(patchNumber);
                    sb.Append($"S{patchLabel}  {singlePatch.Name.Value:10}  ");
                }
                sb.Append("\n");
            }

            sb.Append("\nMULTI patches:\n");

            patchSize = MultiPatch.DataSize;
            var multiPatches = new List<MultiPatch>();
            for (patchNumber = 0; patchNumber < GroupCount * PatchesPerGroup; patchNumber++)
            {
                var multiData = new byte[patchSize];
                Buffer.BlockCopy(this.Data, offset, multiData, 0, patchSize);
                var multiPatch = new MultiPatch(multiData);
                multiPatches.Add(multiPatch);
                offset += patchSize;
            }

            patchNumber = 0;
            for (var rowNumber = 0; rowNumber < PatchesPerGroup; rowNumber++)
            {
                for (var groupNumber = 0; groupNumber < GroupCount; groupNumber++)
                {
                    patchNumber = groupNumber * PatchesPerGroup + rowNumber;
                    var multiPatch = multiPatches[patchNumber];
                    string patchLabel = PatchUtil.GetPatchName(patchNumber);
                    sb.Append($"S{patchLabel}  {multiPatch.Name.Value:10}  ");
                }
                sb.Append("\n");
            }

            sb.Append("\nDRUM:\n");
            byte[] drumData = new byte[DrumPatch.DataSize];
            Buffer.BlockCopy(this.Data, offset, drumData, 0, DrumPatch.DataSize);
            Console.WriteLine($"Constructing drum patch from {drumData.Length} bytes of data starting at {offset}");
            DrumPatch drumPatch = new DrumPatch(drumData);

            sb.Append($"VOLUME = {drumPatch.Volume}  RCV CH = {drumPatch.ReceiveChannel}  VELO DEPTH = {drumPatch.VelocityDepth}\n");
            sb.Append("KEY NAME  NOTE    WAVE S1  WAVE S2  DECAY S1  DECAY S2  TUNE S1  TUNE S2   LEVEL S1  LEVEL S2  SUBMIX CH\n");

            var noteNumber = 36;
            foreach (DrumNote note in drumPatch.Notes)
            {
                sb.Append($"{noteNumber} | {note.Source1.Wave:10} | {note.Source2.Wave:10} | ");
                sb.Append($"{note.Source1.Decay} {note.Source2.Decay} | ");
                sb.Append($"{note.Source1.Tune} {note.Source2.Tune} | ");
                //sb.Append($"{note.Source1.Level} {note.Source2.Level} | ");  // TODO: need to make Level public in KSynthLib!
                sb.Append($"{note.OutputSelect}");

                sb.AppendLine();
                noteNumber++;
            }

            offset += DrumPatch.DataSize;

            sb.Append("\nEFFECT SETTINGS:\n");
            for (var i = 0; i < Bank.EffectPatchCount; i++)
            {
                var effectData = new byte[EffectPatch.DataSize];
                Buffer.BlockCopy(this.Data, offset, effectData, 0, EffectPatch.DataSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                var effectPatch = new EffectPatch(effectData);
                sb.Append($"E-{i+1,2}  {effectPatch}");
                offset += EffectPatch.DataSize;
            }

            return sb.ToString();
        }

        public string HTMLListing()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("<h1>{0}</h1>", this.Title));

            SinglePatch[][] singleBanks = new SinglePatch[BankCount][]; //BankCount, PatchesPerBank];

            int offset = 0;
            int patchSize = SinglePatch.DataSize;

            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                SinglePatch[] patches = new SinglePatch[PatchesPerBank];
                for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
                {
                    var singleData = new byte[patchSize];
                    Buffer.BlockCopy(this.Data, offset, singleData, 0, patchSize);
                    var singlePatch = new SinglePatch(singleData);
                    patches[patchNumber] = singlePatch;
                    offset += patchSize;
                }

                singleBanks[bankNumber] = patches;
            }

            // Now we should have all the single patches collected in four lists of 16 each

            sb.AppendLine("<table>");
            sb.AppendLine("<tr>\n    <th>SINGLE</th>");
            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                char bankLetter = "ABCD"[bankNumber];
                sb.AppendLine(string.Format("    <th>{0}</th>", bankLetter));
            }
            sb.Append("</tr>\n");

            for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("    <td>{0,2}</td>", patchNumber + 1));
                for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
                {
                    SinglePatch[] patches = singleBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    var singlePatch = patches[patchNumber];
                    sb.AppendLine(string.Format($"    <td>{singlePatch.Name:10}</td>"));
                }
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</table>");

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
                    Buffer.BlockCopy(this.Data, offset, multiData, 0, patchSize);
                    var multiPatch = new MultiPatch(multiData);
                    patches[patchNumber] = multiPatch;
                    offset += patchSize;
                }

                multiBanks[bankNumber] = patches;
            }

            sb.AppendLine("<table>");
            sb.AppendLine("<tr>\n    <th>MULTI</th>");
            for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
            {
                char bankLetter = "ABCD"[bankNumber];
                sb.Append(string.Format("    <th>{0}</th>\n", bankLetter));
            }
            sb.AppendLine("</tr>");

            for (var patchNumber = 0; patchNumber < PatchesPerBank; patchNumber++)
            {
                sb.AppendLine("<tr>");
                sb.AppendLine(string.Format("    <td>{0,2}</td>", patchNumber + 1));
                for (var bankNumber = 0; bankNumber < BankCount; bankNumber++)
                {
                    MultiPatch[] patches = multiBanks[bankNumber];
                    string patchId = PatchUtil.GetPatchName(bankNumber * patchNumber);
                    var multiPatch = patches[patchNumber];
                    sb.AppendLine(string.Format($"    <td>{multiPatch.Name:10}</td>"));
                }
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");

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

            sb.AppendLine("<table>");
            sb.AppendLine("<caption>EFFECT</caption>");
            sb.AppendLine("<tr><th>#</th><th>Type and parameters</th></tr>");

            patchSize = EffectPatch.DataSize;

            for (var i = 0; i < Bank.EffectPatchCount; i++)
            {
                var effectData = new byte[patchSize];
                Buffer.BlockCopy(this.Data, offset, effectData, 0, patchSize);
                //Console.WriteLine($"Constructing effect patch from {effectData.Length} bytes of data starting at {offset}");
                var effectPatch = new EffectPatch(effectData);
                sb.AppendLine($"<tr><td>E-{i+1,2}</td><td>{effectPatch}</td></tr>");
                offset += patchSize;
            }

            sb.AppendLine("</table>");

            return sb.ToString();
        }
    }
}
