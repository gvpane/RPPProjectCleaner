# CleanWavFiles

A C# console tool to clean up unused WAV files in Reaper project folders.

## Features
- Deletes or moves unused `.wav` files in the same directory as a `.rpp` file
- Supports batch processing of multiple `.rpp` files with `--multi`
- Can print referenced/unreferenced file lists with `--list`
- Can run without confirmation with `--silent`
- Safe mode: move files to `Unused Wavs` folder instead of deleting

## Usage

### Clean a single project
```
dotnet run --project CleanWavFiles.csproj -- "path/to/project.rpp"
```

### Clean all projects in a directory (recursively)
```
dotnet run --project CleanWavFiles.csproj -- "path/to/projects/folder" --multi
```

### Options
- `--safe`   : Move unused WAVs to `Unused Wavs` folder instead of deleting
- `--list`   : Print lists of referenced and unreferenced files
- `--silent` : Do not prompt for confirmation
- `--multi`  : If path is a directory, process all `.rpp` files recursively

### Example
```
dotnet run --project CleanWavFiles.csproj -- "C:/Music/MySong.rpp" --safe --list
```

## Copilot Instructions
- The main cleaning logic is in `CleanRppFile(string filePath)` in `Program.cs`.
- To add new options, update the argument parsing in `Main`.
- To process all `.rpp` files in a directory, use the `--multi` option and call `RppTreeLister.GetAllRppFiles(dir)`.
- The `RppTreeLister` class in `RppTreeLister.cs` provides a static method to recursively list all `.rpp` files.
- Use early returns for error handling and to keep the code flat (no deep nesting).
- All options can be combined and order does not matter after the path argument.

---

**Warning:** This tool will delete or move files. Always back up your project folders before use!
