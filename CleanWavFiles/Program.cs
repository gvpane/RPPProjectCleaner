namespace CleanWavFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: CleanWavFiles <path-to-rpp-file-or-directory> [options]");
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine("  --unsafe           DELETE unused WAVs permanently (default: move to 'Unused Wavs' folder)");
                Console.WriteLine("  --list             Show list of files before taking action");
                Console.WriteLine("  --dry-run          Preview what would happen without making changes");
                Console.WriteLine("  --silent           Skip confirmation prompt");
                Console.WriteLine("  --multi            Process all .rpp files in directory tree");
                Console.WriteLine("  --exclude-folders  Comma-separated folder names to exclude (e.g., \"Renders,Backup,Archive\")");
                Console.WriteLine();
                Console.WriteLine("Cleanup mode:");
                Console.WriteLine("  --cleanup          Delete all 'Unused Wavs' folders recursively (use with directory path only)");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  CleanWavFiles.exe \"project.rpp\"");
                Console.WriteLine("  CleanWavFiles.exe \"project.rpp\" --dry-run");
                Console.WriteLine("  CleanWavFiles.exe \"project.rpp\" --exclude-folders \"Renders,Backup\"");
                Console.WriteLine("  CleanWavFiles.exe \"D:\\Projects\" --multi --exclude-folders \"Archive\"");
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
                    Console.WriteLine("Error: --cleanup requires a directory path.");
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
                    Console.WriteLine("Directory given, but --multi option not present. Exiting.");
                    return;
                }

                var rppFiles = RppTreeLister.GetAllRppFiles(rppPath);
                if (rppFiles.Count == 0)
                {
                    Console.WriteLine("No .rpp files found in directory.");
                    return;
                }

                foreach (var file in rppFiles)
                {
                    Console.WriteLine($"\nProcessing: {file}");
                    RppCleaner.Clean(file, unsafeMode, listMode, silentMode, dryRunMode, excludedFolders);
                }
                return;
            }

            // Handle single file
            if (!File.Exists(rppPath))
            {
                Console.WriteLine($"RPP file not found: {rppPath}");
                return;
            }

            RppCleaner.Clean(rppPath, unsafeMode, listMode, silentMode, dryRunMode, excludedFolders);
        }
    }
}
