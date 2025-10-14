# Copilot Instructions for RPPProjectCleaner

## Project Overview
This solution contains two C# console tools for managing Reaper DAW project files:
- **CleanWavFiles**: Cleans up unused WAV files from Reaper project (`.rpp`) directories
- **DummyWavMaker**: Generates dummy WAV files for testing purposes

## Architecture & Data Flow

### CleanWavFiles (`CleanWavFiles/Program.cs`)
1. Parses `.rpp` file using regex to extract WAV references: `FILE "([^"]*\.wav)"`
2. Converts relative paths from `.rpp` to absolute paths for accurate matching
3. **Recursively scans ALL subfolders** in project directory for WAV files
4. Supports **folder exclusion** via `--exclude-folders` flag (comma-separated list)
5. Compares full absolute paths to determine unreferenced files
6. **Default behavior**: Moves unreferenced files to "Unused Wavs" folders (safe)
7. **With `--unsafe`**: Deletes unreferenced files permanently
8. Supports batch processing via `RppTreeLister.GetAllRppFiles()` for recursive directory scanning
9. **`--cleanup` mode**: Recursively deletes all "Unused Wavs" folders (standalone operation)

**Key Classes**:
- `RppCleaner.Clean()`: Contains main cleaning logic for processing .rpp files
- `RppCleaner.IsInExcludedFolder()`: Checks if a file path contains any excluded folder names
- `UnusedWavsCleaner.CleanupUnusedWavsFolders()`: Recursively removes all "Unused Wavs" folders

**Path Handling**: 
- WAVs referenced as `"Media\file.wav"` in `.rpp` files are correctly matched against `Media/file.wav` on disk
- Scans recursively with `SearchOption.AllDirectories` 
- Safe mode (default) creates `Unused Wavs` folders preserving original structure
- Example: `Audio/Drums/kick.wav` → `Audio/Drums/Unused Wavs/kick.wav`

### DummyWavMaker (`DummyWavMaker/`)
Generates 10-second mono WAV files (44100Hz, 16-bit) with white noise for testing.
- `ArgumentsValidation.cs`: Validates CLI args and exits with `Environment.Exit(1)` on failure
- `WavFileGenerator.cs`: Creates WAV files with proper RIFF headers using `BinaryWriter`
- Generates random filenames: `Dummy_{random}.wav`

## Code Conventions

### Command-Line Parsing Pattern
```csharp
// Flags extracted using LINQ after position 0 (the path argument)
unsafeMode = args.Skip(1).Contains("--unsafe", StringComparer.OrdinalIgnoreCase);
multiMode = args.Skip(1).Contains("--multi", StringComparer.OrdinalIgnoreCase);
```
All boolean flags are order-independent after the required path argument.

**Special case**: `--cleanup` is a standalone operation that short-circuits normal processing - it only deletes "Unused Wavs" folders and ignores all other flags.

### Error Handling Style
- **Early returns** for error conditions (no deep nesting)
- Console output for user feedback, then `return` (CleanWavFiles) or `Environment.Exit(1)` (DummyWavMaker)
- Example: Check file/directory existence → print error → return

### Static Method Organization
Both projects use static utility classes (`RppTreeLister`, `RppCleaner`, `UnusedWavsCleaner`, `ArgumentsValidation`, `WavFileGenerator`) rather than instantiating objects.

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
# Clean single project (default: move to "Unused Wavs")
dotnet run --project CleanWavFiles -- "path/to/project.rpp" [--list] [--silent]

# Clean single project (unsafe: permanent deletion)
dotnet run --project CleanWavFiles -- "path/to/project.rpp" --unsafe

# Clean all projects in directory tree
dotnet run --project CleanWavFiles -- "path/to/folder" --multi

# Clean with folder exclusion
dotnet run --project CleanWavFiles -- "path/to/project.rpp" --exclude-folders="Renders,Backup"

# Cleanup all "Unused Wavs" folders recursively
dotnet run --project CleanWavFiles -- "path/to/folder" --cleanup

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
