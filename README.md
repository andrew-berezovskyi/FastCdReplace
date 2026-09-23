# FastCdReplace

A Windows application that transforms blocks of positive integers using the project's CD rules. It processes one or two text files from a selected `CD-in` folder and can write both text and binary output. The interface shows progress and optional statistics.

## Requirements and run

Install Windows and the .NET 8 SDK. Open `FastCdReplace.slnx` in a compatible Visual Studio version, or run from the repository root:

```powershell
dotnet run --project FastCdReplace/FastCdReplace.csproj
```

Select a folder containing one or two `.txt` files and start processing. Only files at the top level of the selected folder are read. Two files can be processed concurrently.

## Input and output

Each block starts with the number of values in the block, followed by that many positive integers. Whitespace separates the numbers. For example:

```text
3
2
4
1
```

By default, the app creates `CD-out` beside the selected `CD-in` folder. Each input produces `<name>-CDout.txt` and `<name>-CDout.bin`. The binary format stores a 24-bit block length followed by unary-coded values and is intended for the companion [FastDcReplace](https://github.com/andrew-berezovskyi/FastDcReplace) project.

The specific transformation patterns are implemented in `FastCdReplace/Core/CdTextTransformer.cs`. This is a task-specific educational transformation, not a general file compressor. Disabling output folder creation still processes the input but does not write result files.
