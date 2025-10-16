⚠️ **This tool is extreamly destructive!**

# RPP Project Cleaner

A tool to clean up unused WAV files from your Reaper project folders, helping you recover disk space and keep your projects organized.

## What It Does

CleanWavFiles scans your Reaper project (`.rpp`) files and identifies WAV files that are no longer referenced in your project. By default, it **safely moves** these unused files to an "Unused Wavs" folder so you can review them before permanently deleting them.

## Features

- **Safe by default**: Moves unused files to a separate folder instead of deleting them
- **Multi-project awareness**: Automatically detects multiple .rpp files (versions, backups) and aggregates references
- **Recursive scanning**: Scans all subfolders in your project directory to find WAV files
- **File size tracking**: Shows individual file sizes and total space to be recovered
- **Color-coded output**: Visual feedback with colors (green=safe, yellow=moving, red=deleting)
- **Dry-run mode**: Preview exactly what would happen without making any changes
- **Orphaned project detection**: Warns about .rpp files with missing WAV references
- **Flexible exclusion**: Skip specific folders like "Renders" or "Backup" with `--exclude-folders`
- **Batch processing**: Clean multiple projects at once with `--multi`
- **Detailed summary**: Shows statistics about files processed and space recovered
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

**Note:** If multiple .rpp files are detected in the directory (e.g., `MySong.rpp`, `MySong_v2.rpp`, `MySong_backup.rpp`), the tool will prompt you to include all of them when checking references. This prevents accidentally removing WAVs that are used in other versions.

### Clean Multiple Projects

Process all `.rpp` files in a directory and its subdirectories:
```
CleanWavFiles.exe "C:\Music\MyProjects" --multi
```

### Dry Run - Preview Without Changes

See exactly what would happen without making any changes (recommended first use):
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --dry-run
```

This shows file sizes, total space, which .rpp files were scanned, and what would be moved/deleted, but doesn't actually do anything.

### Include All Project Versions Automatically

Skip the prompt and automatically include all .rpp files in the directory:
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --include-all-rpp
```

This is useful when you know you have multiple versions and want them all considered.

### Detailed Preview Before Cleaning

See which files will be moved with file sizes before confirming:
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
| `--dry-run` | Preview what would happen without making any changes (shows sizes and multi-rpp detection) |
| `--list` | Show detailed list of files with sizes before taking action |
| `--silent` | Skip confirmation prompts (auto-confirms multi-rpp inclusion) |
| `--multi` | Process all `.rpp` files in a directory tree |
| `--include-all-rpp` | Automatically include all .rpp files in directory (no prompt) |
| `--unsafe` | **Permanently delete** unused files instead of moving them ⚠️ |
| `--exclude-folders="Name1,Name2"` | Skip scanning specific folder names (comma-separated) |

These options can be combined in any order after the file/folder path.

### Cleanup Mode
| Option | Description |
|--------|-------------|
| `--cleanup` | Delete all "Unused Wavs" folders recursively (standalone operation) |

**Note:** `--cleanup` is a standalone operation and cannot be combined with other options. It requires a directory path.

## Examples

**First time use - Dry run to see what would happen:**
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --dry-run
```

**Safe cleanup with detailed preview:**
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

**Auto-include all .rpp versions without prompting:**
```
CleanWavFiles.exe "C:\Music\MySong.rpp" --include-all-rpp --silent
```

**Clean up all "Unused Wavs" folders after review:**
```
CleanWavFiles.exe "C:\Music\2024" --cleanup
```

## Important Notes

⚠️ **Always back up your projects before using this tool!**

- **Multi-project protection**: When multiple .rpp files exist in a directory (versions, backups), the tool prompts to include all of them when checking references, preventing accidental deletion of WAVs used in other versions
- **Recursive scanning**: The tool scans ALL subfolders in your project directory by default (including subdirectories for .rpp files)
- **Orphaned project detection**: Warns you about .rpp files that reference missing WAV files (possibly old/broken versions)
- **Preserves context**: Files are moved to "Unused Wavs" folders in their original location
  - Example: `Audio/Drums/kick.wav` → `Audio/Drums/Unused Wavs/kick.wav`
- **Folder exclusion**: Use `--exclude-folders` to skip specific directories (e.g., renders, backups)
- **WAV-only**: Only processes WAV files that are not referenced in any .rpp file
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
