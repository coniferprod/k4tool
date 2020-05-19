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

        [Option('p', "patch", Required = true, HelpText = "Patch bank and number (for example, A-1).")]
        public string PatchNumber { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName { get; set; }
    }
}