# Copilot Instructions for RPPProjectCleaner

## Project Overview
This solution contains two C# console tools for managing Reaper DAW project files:
- **CleanWavFiles**: Cleans up unused WAV files from Reaper project (`.rpp`) directories
- **DummyWavMaker**: Generates dummy WAV files for testing purposes

## Architecture & Data Flow

### CleanWavFiles (`CleanWavFiles/Program.cs`)
1. Parses `.rpp` file using regex to extract WAV references: `FILE "([^"]*\.wav)"`
2. Converts relative paths from `.rpp` to absolute paths for accurate matching
3. Scans both project root directory AND `Media/` subfolder for WAV files
4. Compares full absolute paths to determine unreferenced files
5. Deletes or moves unreferenced files, preserving original directory structure
6. Supports batch processing via `RppTreeLister.GetAllRppFiles()` for recursive directory scanning

**Key Method**: `CleanRppFile(string filePath)` - contains all cleaning logic with local copies of global flags

**Path Handling**: WAVs referenced as `"Media\file.wav"` in `.rpp` files are correctly matched against `Media/file.wav` on disk. Safe mode creates separate `Unused Wavs` folders in each location (root and Media).

### DummyWavMaker (`DummyWavMaker/`)
Generates 10-second mono WAV files (44100Hz, 16-bit) with white noise for testing.
- `ArgumentsValidation.cs`: Validates CLI args and exits with `Environment.Exit(1)` on failure
- `WavFileGenerator.cs`: Creates WAV files with proper RIFF headers using `BinaryWriter`
- Generates random filenames: `Dummy_{random}.wav`

## Code Conventions

### Command-Line Parsing Pattern
```csharp
// Flags extracted using LINQ after position 0 (the path argument)
safeMode = args.Skip(1).Contains("--safe", StringComparer.OrdinalIgnoreCase);
multiMode = args.Skip(1).Contains("--multi", StringComparer.OrdinalIgnoreCase);
```
All boolean flags are order-independent after the required path argument.

### Error Handling Style
- **Early returns** for error conditions (no deep nesting)
- Console output for user feedback, then `return` (CleanWavFiles) or `Environment.Exit(1)` (DummyWavMaker)
- Example: Check file/directory existence → print error → return

### Static Method Organization
Both projects use static utility classes (`RppTreeLister`, `ArgumentsValidation`, `WavFileGenerator`) rather than instantiating objects.

## Developer Workflows

### Build
```powershell
# Build individual projects
dotnet build CleanWavFiles/CleanWavFiles.csproj
dotnet build DummyWavMaker/DummyWavMaker.csproj

# Or use VS Code tasks (see .vscode/tasks.json)
```

### Run
```powershell
# Clean single project
dotnet run --project CleanWavFiles -- "path/to/project.rpp" [--safe] [--list] [--silent]

# Clean all projects in directory tree
dotnet run --project CleanWavFiles -- "path/to/folder" --multi --safe

# Generate test files
dotnet run --project DummyWavMaker -- "path/to/output" 50
```

## Project-Specific Patterns

### Nullable Reference Types
- CleanWavFiles: `<Nullable>disable</Nullable>` - legacy style with null checks
- DummyWavMaker: `<Nullable>enable</Nullable>` - modern nullable annotations

### Regex Usage
Use `RegexOptions.IgnoreCase` for file matching since Windows is case-insensitive but .rpp files may have mixed-case references.

### File Operations
- Use `Path.GetFullPath()` for absolute path comparisons with `StringComparer.OrdinalIgnoreCase`
- Normalize path separators: `.Replace('/', '\\')` to handle both Windows and cross-platform paths in .rpp files
- `File.Move(source, dest, overwrite: true)` when moving to safe folder
- `Directory.GetFiles(dir, "*.wav", SearchOption.TopDirectoryOnly)` - check both root and `Media/` subdirectory
- Safe mode preserves directory structure: root files → `root/Unused Wavs/`, Media files → `Media/Unused Wavs/`
- Use `Path.GetRelativePath(rppDir, wavFile)` for user-friendly console output

## Testing Strategy
`TestProgram.cs` exists but is empty. Use `DummyWavMaker` to create test scenarios:
1. Generate dummy WAVs in a test directory
2. Create a `.rpp` file referencing some (but not all) WAVs
3. Run CleanWavFiles with `--safe --list` to verify detection logic
