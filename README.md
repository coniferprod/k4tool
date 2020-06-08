# k4tool

Patch management utilities for the Kawai K4 synthesizer.

## Command-line arguments with dotnet

To pass command-line arguments to the k4tool instead of having the `dotnet`
tool implement them, use the `--` argument, like this:

    dotnet run -- -help

