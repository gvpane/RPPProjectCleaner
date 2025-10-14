namespace CleanWavFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleHelper.WriteInfo("Usage: CleanWavFiles <path-to-rpp-file-or-directory> [options]");
                ConsoleHelper.WriteInfo();
                ConsoleHelper.WriteInfo("Options:");
                ConsoleHelper.WriteInfo("  --unsafe           DELETE unused WAVs permanently (default: move to 'Unused Wavs' folder)");
                ConsoleHelper.WriteInfo("  --list             Show list of files before taking action");
                ConsoleHelper.WriteInfo("  --dry-run          Preview what would happen without making changes");
                ConsoleHelper.WriteInfo("  --silent           Skip confirmation prompt");
                ConsoleHelper.WriteInfo("  --multi            Process all .rpp files in directory tree");
                ConsoleHelper.WriteInfo("  --exclude-folders  Comma-separated folder names to exclude (e.g., \"Renders,Backup,Archive\")");
                ConsoleHelper.WriteInfo();
                ConsoleHelper.WriteInfo("Cleanup mode:");
                ConsoleHelper.WriteInfo("  --cleanup          Delete all 'Unused Wavs' folders recursively (use with directory path only)");
                ConsoleHelper.WriteInfo();
                ConsoleHelper.WriteInfo("Examples:");
                ConsoleHelper.WriteInfo("  CleanWavFiles.exe \"project.rpp\"");
                ConsoleHelper.WriteInfo("  CleanWavFiles.exe \"project.rpp\" --dry-run");
                ConsoleHelper.WriteInfo("  CleanWavFiles.exe \"project.rpp\" --exclude-folders \"Renders,Backup\"");
                ConsoleHelper.WriteInfo("  CleanWavFiles.exe \"D:\\Projects\" --multi --exclude-folders \"Archive\"");
                return;
            }

            string rppPath = args[0];

            // Parse option flags (can appear in any order after the path)
            bool cleanupMode = args.Skip(1).Contains("--cleanup", StringComparer.OrdinalIgnoreCase);
            bool unsafeMode = args.Skip(1).Contains("--unsafe", StringComparer.OrdinalIgnoreCase);
            bool listMode = args.Skip(1).Contains("--list", StringComparer.OrdinalIgnoreCase);
            bool dryRunMode = args.Skip(1).Contains("--dry-run", StringComparer.OrdinalIgnoreCase);
            bool silentMode = args.Skip(1).Contains("--silent", StringComparer.OrdinalIgnoreCase);
            bool multiMode = args.Skip(1).Contains("--multi", StringComparer.OrdinalIgnoreCase);

            // Dry-run mode implies list mode and prevents actual changes
            if (dryRunMode)
            {
                listMode = true;
                silentMode = true; // Skip confirmation since we're not doing anything
            }

            // Parse excluded folders
            HashSet<string> excludedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var excludeArg = args.Skip(1).FirstOrDefault(arg => arg.StartsWith("--exclude-folders", StringComparison.OrdinalIgnoreCase));
            if (excludeArg != null)
            {
                int equalsIndex = excludeArg.IndexOf('=');
                if (equalsIndex > 0 && equalsIndex < excludeArg.Length - 1)
                {
                    string folderList = excludeArg.Substring(equalsIndex + 1).Trim('"', '\'');
                    foreach (var folder in folderList.Split(','))
                    {
                        string trimmed = folder.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            excludedFolders.Add(trimmed);
                        }
                    }
                }
            }

            // Handle cleanup mode (independent operation)
            if (cleanupMode)
            {
                if (!Directory.Exists(rppPath))
                {
                    ConsoleHelper.WriteError("Error: --cleanup requires a directory path.");
                    return;
                }

                UnusedWavsCleaner.CleanupUnusedWavsFolders(rppPath);
                return;
            }

            // Handle directory (multi-mode)
            if (Directory.Exists(rppPath))
            {
                if (!multiMode)
                {
                    ConsoleHelper.WriteError("Directory given, but --multi option not present. Exiting.");
                    return;
                }

                var rppFiles = RppTreeLister.GetAllRppFiles(rppPath);
                if (rppFiles.Count == 0)
                {
                    ConsoleHelper.WriteError("No .rpp files found in directory.");
                    return;
                }

                foreach (var file in rppFiles)
                {
                    ConsoleHelper.WriteInfo($"Processing: {file}");
                    RppCleaner.Clean(file, unsafeMode, listMode, silentMode, dryRunMode, excludedFolders);
                }
                return;
            }

            // Handle single file
            if (!File.Exists(rppPath))
            {
                ConsoleHelper.WriteError($"RPP file not found: {rppPath}");
                return;
            }

            RppCleaner.Clean(rppPath, unsafeMode, listMode, silentMode, dryRunMode, excludedFolders);
        }
    }
}
