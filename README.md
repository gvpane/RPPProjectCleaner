⚠️ **This tool is extreamly destructive!**

# RPP Project Cleaner

A tool to clean up unused WAV files from your Reaper project folders, helping you recover disk space and keep your projects organized.

## What It Does

CleanWavFiles scans your Reaper project (`.rpp`) files and identifies WAV files that are no longer referenced in your project. By default, it **safely moves** these unused files to an "Unused Wavs" folder so you can review them before permanently deleting them.

## Features

- **Safe by default**: Moves unused files to a separate folder instead of deleting them
- **Recursive scanning**: Scans all subfolders in your project directory to find WAV files
- **Flexible exclusion**: Skip specific folders like "Renders" or "Backup" with `--exclude-folders`
- **Batch processing**: Clean multiple projects at once with `--multi`
- **Preview mode**: See what will be moved before taking action with `--list`
- **No interruptions**: Skip confirmation prompts with `--silent`
- **Preserves structure**: Maintains folder context when moving files to "Unused Wavs" folders

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the executable to a folder of your choice
3. Run from command line or PowerShell

## Usage

### Basic Usage

Clean a single Reaper project (moves unused WAVs to "Unused Wavs" folder):
```
CleanWavFiles.exe "C:\Music\MySong.rpp"
```

### Clean Multiple Projects

Process all `.rpp` files in a directory and its subdirectories:
```
CleanWavFiles.exe "C:\Music\MyProjects" --multi
```

### Preview Before Cleaning

See which files will be moved before confirming:
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --list
```

### Batch Processing Without Prompts

Useful for automated cleanup workflows:
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --silent
```

### Exclude Specific Folders

If you have folders you want to skip (like renders, backups, or archives):
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --exclude-folders="Renders,Backup,Archive"
```

Works with multi-mode too:
```
CleanWavFiles.exe "C:\Music\MyProjects" --multi --exclude-folders="Renders,Stems"
```

### Permanent Deletion (Use with Caution!)

If you want to delete unused files immediately instead of moving them:
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --unsafe
```

### Cleanup "Unused Wavs" Folders

After reviewing the moved files, delete all "Unused Wavs" folders in a directory tree:
```
CleanWavFiles.exe "C:\Music\MyProjects" --cleanup
```

This will recursively find and delete all "Unused Wavs" folders and their contents. No confirmation is required.

## Options

### File Cleaning Options
| Option | Description |
|--------|-------------|
| `--list` | Show which files are referenced and which will be moved/deleted |
| `--silent` | Skip the confirmation prompt (auto-confirm) |
| `--multi` | Process all `.rpp` files in a directory tree |
| `--unsafe` | **Permanently delete** unused files instead of moving them ⚠️ |
| `--exclude-folders="Name1,Name2"` | Skip scanning specific folder names (comma-separated) |

These options can be combined in any order after the file/folder path.

### Cleanup Mode
| Option | Description |
|--------|-------------|
| `--cleanup` | Delete all "Unused Wavs" folders recursively (standalone operation) |

**Note:** `--cleanup` is a standalone operation and cannot be combined with other options. It requires a directory path.

## Examples

**Safe cleanup with preview:**
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --list
```

**Batch cleanup of all projects in a folder:**
```
CleanWavFiles.exe "C:\Music\2024" --multi --silent
```

**Cleanup with permanent deletion (careful!):**
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --unsafe --list
```

**Exclude specific folders from scanning:**
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --exclude-folders="Renders,Backup,Archive"
```

**Clean up all "Unused Wavs" folders after review:**
```
CleanWavFiles.exe "C:\Music\2024" --cleanup
```

## Important Notes

⚠️ **Always back up your projects before using this tool!**

- **Recursive scanning**: The tool now scans ALL subfolders in your project directory by default
- **Preserves context**: Files are moved to "Unused Wavs" folders in their original location
  - Example: `Audio/Drums/kick.wav` → `Audio/Drums/Unused Wavs/kick.wav`
- **Folder exclusion**: Use `--exclude-folders` to skip specific directories (e.g., renders, backups)
- **WAV-only**: Only processes WAV files that are not referenced in the `.rpp` file
- **Permanent deletion**: Using `--unsafe` will permanently delete files - they cannot be recovered!

## Building from Source

If you want to compile the tool yourself:

```powershell
git clone https://github.com/gvpane/RPPProjectCleaner.git
cd RPPProjectCleaner
dotnet build CleanWavFiles/CleanWavFiles.csproj
```

Run the development version:
```powershell
dotnet run --project CleanWavFiles -- "path/to/project.rpp" [options]
```

## License

This project is open source. See LICENSE file for details.
