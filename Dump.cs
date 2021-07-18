
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
            return String.Format($"{label,-10}{value}");
        }

        private string MakeTwoColumnRow(string category, string label, string value, bool isFirst = false)
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

        private string CenteredString(string s, int desiredLength)
        {
            if (s.Length >= desiredLength)
            {
                return s;
            }
            int firstPad = (s.Length + desiredLength) / 2;
            return s.PadLeft(firstPad).PadRight(desiredLength);
        }

        public string MakeSinglePatchText(SinglePatch singlePatch)
        {

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

        public string MakeMultiPatchText(MultiPatch multiPatch)
        {
            List<string> lines = new List<string>();

            lines.Add(MakeSingleColumnRow("Volume", multiPatch.Volume.ToString()));
            lines.Add(MakeSingleColumnRow("Effect", multiPatch.EffectPatch.ToString()));
            lines.Add(MakeSingleColumnRow("Name", multiPatch.Name.ToString()));

            string space = " ";
            lines.Add(String.Format("{0,-30}{1}{2}{3}{4}{5}{6}{7}{8}", space,
                CenteredString("1", 5), CenteredString("2", 5), CenteredString("3", 5), CenteredString("4", 5),
                CenteredString("5", 5), CenteredString("6", 5), CenteredString("7", 5), CenteredString("8", 5)));

            Dictionary<String, String> patchNames = new Dictionary<String, String>();

            StringBuilder sectionValues = new StringBuilder();
            foreach (Section section in multiPatch.sections)
            {
                string number = PatchUtil.GetPatchName(section.SinglePatch).Replace(" ", String.Empty);
                sectionValues.Append(CenteredString(number, 5));

                if (!patchNames.ContainsKey(number))
                {
                    patchNames.Add(number, this.bank.Singles[section.SinglePatch].Name);
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
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.ZoneLow.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Zone Lo", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.ZoneHigh.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Zone Hi", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.VelocitySwitch.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Zone", "Vel Sw", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.ReceiveChannel.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Sec Ch", "Rcv Ch", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.PlayMode.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Sec Ch", "Mode", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.Level.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Level", sectionValues.ToString(), true));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.Transpose.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Trans", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.Tune.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Tune", sectionValues.ToString()));

            sectionValues.Clear();
            foreach (Section section in multiPatch.sections)
            {
                sectionValues.Append(CenteredString(section.Output.ToString(), 5));
            }
            lines.Add(MakeTwoColumnRow("Output", "Submix Ch", sectionValues.ToString()));

            // Add names of singles used by this multi:
            StringBuilder patchNamesLine = new StringBuilder();
            foreach (string number in patchNames.Keys)
            {
                string name = patchNames[number];
                patchNamesLine.Append(String.Format($"{number} = {name}  "));
            }
            lines.Add(patchNamesLine.ToString());

            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
