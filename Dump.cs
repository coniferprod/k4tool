
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
    public class Dump
    {
        private Bank bank;

        public Dump(Bank bank)
        {
            this.bank = bank;
        }

        private string MakeSingleColumnRow(string label, string value)
        {
            return $"{label,-10}{value}";
        }

        private string MakeTwoColumnRow(string category, string label, string value, bool isFirst = false)
        {
            var sb = new StringBuilder();

            string space = " ";
            if (isFirst)
            {
                sb.Append($"{category,-10}");
            }
            else
            {
                sb.Append($"{space,-10}");
            }

            sb.Append($"{label,-20} {value}");

            return sb.ToString();
        }

        private string CenteredString(string s, int desiredLength)
        {
            if (s.Length >= desiredLength)
            {
                return s;
            }
            var firstPad = (s.Length + desiredLength) / 2;
            return s.PadLeft(firstPad).PadRight(desiredLength);
        }

        public string MakeSinglePatchText(SinglePatch singlePatch)
        {

            var lines = new List<string>();

            lines.Add(MakeSingleColumnRow("Volume", singlePatch.Volume.ToString()));
            lines.Add(MakeSingleColumnRow("Effect", singlePatch.Effect.ToString()));
            lines.Add(MakeSingleColumnRow("Submix ch", singlePatch.Submix.ToString()));
            lines.Add(MakeSingleColumnRow("Name", singlePatch.Name));
            lines.Add(MakeTwoColumnRow("Common", "Source Mode", singlePatch.SourceMode.ToString(), true));

            var amValue = new StringBuilder();
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

            var space = " ";
            lines.Add(String.Format("{0,-30}{1}{2}{3}{4}", space, CenteredString("S1", 10), CenteredString("S2", 10), CenteredString("S3", 10), CenteredString("S4", 10)));

            var sourceValues = new StringBuilder();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Delay.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "Delay", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.VelocityCurve.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "Vel curve", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.KeyScalingCurve.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("S-Common", "KS curve", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Wave.Number.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Wave", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.KeyTrack.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Key Track", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Coarse.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Coarse", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Fine.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Fine", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.PressureFrequency.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Press freq", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var source in singlePatch.Sources)
            {
                sourceValues.Append(CenteredString(source.Vibrato.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCO", "Vib/A.bend", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.EnvelopeLevel.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Level", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Attack.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Attack", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Decay.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Decay", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Sustain.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Sustain", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.Env.Release.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA", "Release", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.VelocityDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Vel Depth", sourceValues.ToString(), true));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.PressureDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Press Depth", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.LevelMod.KeyScalingDepth.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "KS Depth", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.AttackVelocity.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "Time Mod Attack", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.ReleaseVelocity.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "         Release", sourceValues.ToString(), false));

            sourceValues.Clear();
            foreach (var amplifier in singlePatch.Amplifiers)
            {
                sourceValues.Append(CenteredString(amplifier.TimeMod.KeyScaling.ToString(), 10));
            }
            lines.Add(MakeTwoColumnRow("DCA Mod", "         KS", sourceValues.ToString(), false));

            lines.Add(string.Format("{0,-30}{1}{2}", space, CenteredString("F1", 20), CenteredString("F2", 20)));

            lines.Add(MakeTwoColumnRow("DCF", "Cutoff",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Cutoff.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Cutoff.ToString(), 20)), true));

            lines.Add(MakeTwoColumnRow("DCF", "Resonance",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Resonance.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Resonance.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "Vel Depth",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.CutoffMod.VelocityDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.CutoffMod.VelocityDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "KS Depth",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.CutoffMod.KeyScalingDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.CutoffMod.KeyScalingDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF", "LFO",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.IsLFO.ToString(), 20),
                    CenteredString(singlePatch.Filter2.IsLFO.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Env Depth",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.EnvelopeDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.EnvelopeDepth.ToString(), 20)), true));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Vel Depth",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.EnvelopeVelocityDepth.ToString(), 20),
                    CenteredString(singlePatch.Filter2.EnvelopeVelocityDepth.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Attack",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Attack.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Attack.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Decay",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Decay.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Decay.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Sustain",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Sustain.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Sustain.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Release",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.Env.Release.ToString(), 20),
                    CenteredString(singlePatch.Filter2.Env.Release.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "Time Mod Attack",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.AttackVelocity.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.AttackVelocity.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "         Release",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.ReleaseVelocity.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.ReleaseVelocity.ToString(), 20))));

            lines.Add(MakeTwoColumnRow("DCF Mod", "         KS",
                string.Format("{0}{1}",
                    CenteredString(singlePatch.Filter1.TimeMod.KeyScaling.ToString(), 20),
                    CenteredString(singlePatch.Filter2.TimeMod.KeyScaling.ToString(), 20))));

            var sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }

        public string MakeMultiPatchText(MultiPatch multiPatch)
        {
            var lines = new List<string>();

            lines.Add(MakeSingleColumnRow("Volume", multiPatch.Volume.ToString()));
            lines.Add(MakeSingleColumnRow("Effect", multiPatch.EffectPatch.ToString()));
            lines.Add(MakeSingleColumnRow("Name", multiPatch.Name.ToString()));

            var space = " ";
            lines.Add(string.Format("{0,-30}{1}{2}{3}{4}{5}{6}{7}{8}", space,
                CenteredString("1", 5), CenteredString("2", 5), CenteredString("3", 5), CenteredString("4", 5),
                CenteredString("5", 5), CenteredString("6", 5), CenteredString("7", 5), CenteredString("8", 5)));

            var patchNames = new Dictionary<String, String>();

            var sectionValues = new StringBuilder();
            foreach (var section in multiPatch.Sections)
            {
                //Console.WriteLine($"Section single patch = {section.SinglePatch.Value}");

                string number = PatchUtil.GetPatchName(section.SinglePatch - 1).Replace(" ", string.Empty);
                sectionValues.Append(CenteredString(number, 5));

                if (!patchNames.ContainsKey(number))
                {
                    patchNames.Add(number, this.bank.Singles[section.SinglePatch - 1].Name);
                }
            }
            lines.Add(MakeTwoColumnRow("Inst", "Single Number", sectionValues.ToString(), true));

/*
            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(singlePatches[section.SinglePatch].Name, 5));
            }
            lines.Add(MakeTwoColumnRow("Inst", "       Name", sectionValues.ToString(), true));
*/

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.KeyboardZone.Low.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Zone Lo", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.KeyboardZone.High.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Zone Hi", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.VelocitySwitch.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Vel Sw", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.ReceiveChannel.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Sec Ch", "Rcv Ch", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.PlayMode.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Sec Ch", "Mode", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.Level.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Level", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.Transpose.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Trans", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.Tune.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Tune", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (var section in multiPatch.Sections)
            {
                sectionValues.Append(CenteredString(section.Output.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Submix Ch", sectionValues.ToString()));

            // Add names of singles used by this multi:
            var patchNamesLine = new StringBuilder();
            foreach (string number in patchNames.Keys)
            {
                string name = patchNames[number];
                patchNamesLine.Append($"{number} = {name}  ");
            }
            lines.Add(patchNamesLine.ToString());

            var sb = new StringBuilder();

            foreach (string line in lines)
            {
                sb.AppendLine(line);
            }

            return sb.ToString();
        }
    }
}
