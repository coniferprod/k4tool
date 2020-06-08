using System;
using CommandLine;

namespace K4Tool
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

        [Option('o', "output", Required = true, HelpText = "Output file format ('text' or 'json')")]
        public string Output { get; set; }
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
    public class ExtractOptions {
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
}