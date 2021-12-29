using System;
using CommandLine;

namespace K4Tool
{
    [Verb("list", HelpText = "List the contents of a bank.")]
    public class ListOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'html')")]
        public string Output { get; set; }
    }

    [Verb("dump", HelpText = "Dump the contents of a bank.")]
    public class DumpOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Input file to be processed.")]
        public string FileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'json')")]
        public string Output { get; set; }
    }

    [Verb("init", HelpText = "Initialize a new bank.")]
    public class InitOptions
    {
        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName { get; set; }
    }

    [Verb("generate", HelpText = "Generate a new patch.")]
    public class GenerateOptions
    {
        [Option('t', "type", Required = true, HelpText = "Type of patch (currently: single).")]
        public string PatchType { get; set; }

        [Option('n', "name", Required = true, HelpText = "Name of patch (max 10 characters).")]
        public string PatchName { get; set; }

        [Option('p', "patch", Required = true, HelpText = "Patch bank and number (for example, A1 or D16).")]
        public string PatchNumber { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName { get; set; }
    }

    [Verb("extract", HelpText = "Extract patch from bank.")]
    public class ExtractOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input file to extract from. Must be a bank file.")]
        public string InputFileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file to extract to.")]
        public string OutputFileName { get; set; }

        [Option('t', "type", Required = true, HelpText = "Type of patch (single, multi, drum, effect).")]
        public string PatchType { get; set; }

        [Option('s', "source", Required = true, HelpText = "Source patch bank and number (for example, A1 or D16).")]
        public string SourcePatchNumber { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination patch bank and number (for example, A1 or D16).")]
        public string DestinationPatchNumber { get; set; }
    }

    [Verb("inject", HelpText = "Inject patch into bank.")]
    public class InjectOptions
    {
        [Option('i', "input", Required = true, HelpText = "Patch file whose contents to inject. Must be a single, multi, drum, or effect.")]
        public string InputFileName { get; set; }

        [Option('t', "target", Required = true, HelpText = "Target bank to inject to.")]
        public string TargetFileName { get; set; }

        [Option('d', "destination", Required = false, HelpText = "Destination patch bank and number (for example, A1 or D16). If omitted, the bank and number in the patch file are used, if applicable.")]
        public string DestinationPatchNumber { get; set; }
    }

    [Verb("copy", HelpText = "Copy patch from one bank to another.")]
    public class CopyOptions
    {
        [Option('i', "input", Required = true, HelpText = "Name of input bank file")]
        public string InputFileName { get; set; }

        [Option('t', "type", Required = true, HelpText = "Type of patch (single, multi, drum, effect).")]
        public string PatchType { get; set; }

        [Option('s', "source", Required = true, HelpText = "Source patch bank and number (for example, A1 or D16).")]
        public string SourcePatchNumber { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file to extract to.")]
        public string OutputFileName { get; set; }

        [Option('d', "destination", Required = true, HelpText = "Destination patch bank and number (for example, A1 or D16).")]
        public string DestinationPatchNumber { get; set; }

        [Option('e', "effect", Required = false, HelpText = "Effect number (1...32)")]
        public int EffectNumber { get; set; }
    }

    [Verb("wave", HelpText = "Show wave list.")]
    public class WaveOptions {
        // empty
    }

    [Verb("table", HelpText = "Create patch tables")]
    public class TableOptions {
        [Option('f', "format", Required = true, HelpText = "Output file format ('docbook' or 'html').")]
        public string Format { get; set; }

        [Option('p', "patch", Required = true, HelpText = "Type of patch (all, single, multi, drum, effect.")]
        public string PatchType { get; set; }

        [Option('i', "input", Required = true, HelpText = "Input file name.")]
        public string InputFileName { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file name.")]
        public string OutputFileName { get; set; }
    }

    [Verb("info", HelpText = "Show information about System Exclusive file")]
    public class InfoOptions {
        [Option('i', "input", Required = true, HelpText = "Input file name.")]
        public string InputFileName { get; set; }
    }
}
