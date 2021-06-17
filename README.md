# k4tool

Patch management utilities for the Kawai K4 synthesizer.

## Command-line arguments with dotnet

To pass command-line arguments to the k4tool instead of having the `dotnet`
tool implement them, use the `--` argument, like this:

    dotnet run -- --help

## Examples

This section contains command-line examples of various k4tool commands.
You can use either the long or short format of the argument.

### List the patches in a bank

The `list` command produces a listing of all the patches in a given bank.
The input file must be a complete System Exclusive bank file of 15,123 bytes.
For example, to get a text listing of the patches in the bank file `A401.SYX`,
printed to the console, you would say:

    dotnet run -- list --filename A401.SYX --output text

### Extract a patch from a bank

You can use the `extract` command to extract a patch from a bank into a separate
file. For example, to extract patch A-8 from the bank A401.SYX you would say:

    dotnet run -- extract --input A401.SYX --output a401-a8.syx --type single --source a8 --destination a8

You need to specify the type of the patch to distinguish for example the single patch A-8
from the multi patch A-8.

The `--destination` option imprints the patch number into the resulting System Exclusive
message file, so that you can use any capable utility to transfer the file to the K4.

### Injecting a patch into a bank

You can use the `inject` command to inject a patch from a System Exclusive file into
a bank, replacing an existing patch. For example, if the file `a401-a8.syx` contains a
single patch, you can inject it into the bank file `A402.SYX` into slot B-6 with this command:

    dotnet run -- inject --input a401-a8.syx --target A402.SYX --destination b6

If you leave out the optional `destination` argument, the patch will be injected into the
slot that is imprinted in the patch file, either by the `k4tool` `extract` command or a manual
data dump from the K4, captured by some other utility.

