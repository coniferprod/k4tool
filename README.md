# k4tool

Utility to work with Kawai K4 synthesizer patches.

## Installation

You will need [.NET 6](https://dotnet.microsoft.com/en-us/download) or later from Microsoft to run this program. There is no binary distribution, so you'll need to build the program before you can run it.

Quick instructions for macOS:
- Install .NET.
- You may already have Git installed. If not, use Homebrew to install it.
- Open Terminal.
- Clone the repository using Git, with `git clone https://github.com/coniferprod/k4tool.git`.
- Change to the directory with `cd k4tool`.
- Run the program with `dotnet run` and view the command-line options.

## Usage

For all commands and their parameters, use `dotnet run -- help`. For information about a specific command,
use `dotnet run -- help command`, where command is `list`, `dump`, `info` etc. Not all commands are
completely functional yet, if they ever will be.

### Listing the contents of a bank

Use the `list` command to list the singles, multis, drum and effects in a bank.

### Information about a System Exclusive file

Use the `info` command to identify a Kawai System Exclusive file.

### Extracting individual patches

Use the `extract` command to save individual patches from a bank into a separate file.

### Showing the wave list

To see a list of the Kawai K4 waves, use the `wave` command.

## Disclaimer

This is not a polished end-user product. Use it at your own risk. If it works for you, great.
If it doesn't, you can improve it or fix it if you know how to program in C# for .NET.

You can also send suggestions for improvements of features, but I won't promise to do anything about them.
