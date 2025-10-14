using System.Text.RegularExpressions;

namespace CleanWavFiles
{
    static class RppCleaner
    {
        public static void Clean(string filePath, bool unsafeMode, bool listMode, bool silentMode, bool dryRunMode, HashSet<string> excludedFolders = null)
        {
            excludedFolders ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string rppDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(rppDir))
            {
                ConsoleHelper.WriteError($"Could not determine directory for: {filePath}");
                return;
            }

            string rppContent = File.ReadAllText(filePath);
            
            // Extract relative paths from .rpp and convert to absolute paths
            var referencedWavPaths = Regex.Matches(rppContent, "FILE \"([^\"]*\\.wav)\"", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Replace('/', '\\'))  // Normalize path separators
                .Select(relativePath => Path.GetFullPath(Path.Combine(rppDir, relativePath)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            ConsoleHelper.WriteInfo($"Found {referencedWavPaths.Count} referenced WAV files in the project");
            
            if (listMode && !dryRunMode)
            {
                foreach (var refWav in referencedWavPaths.OrderBy(x => x))
                {
                    long fileSize = File.Exists(refWav) ? new FileInfo(refWav).Length : 0;
                    ConsoleHelper.WriteSuccess($"  ✓ {Path.GetRelativePath(rppDir, refWav)} ({ConsoleHelper.FormatFileSize(fileSize)})");
                }
                Console.WriteLine();
            }

            // Collect WAV files recursively, excluding specified folders
            var wavFiles = Directory.GetFiles(rppDir, "*.wav", SearchOption.AllDirectories)
                .Where(wavFile => !IsInExcludedFolder(wavFile, rppDir, excludedFolders))
                .ToList();

            if (excludedFolders.Count > 0)
            {
                ConsoleHelper.WriteInfo($"Found {wavFiles.Count} WAV files in directory tree (excluding: {string.Join(", ", excludedFolders)})");
            }
            else
            {
                ConsoleHelper.WriteInfo($"Found {wavFiles.Count} WAV files in directory tree");
            }

            // Compare full absolute paths and calculate sizes
            var filesToDelete = wavFiles
                .Where(wavFile => !referencedWavPaths.Contains(Path.GetFullPath(wavFile)))
                .ToList();

            if (filesToDelete.Count == 0)
            {
                ConsoleHelper.WriteSuccess("✓ No unused .wav files found. All files are referenced!");
                return;
            }

            // Calculate total size
            long totalSize = filesToDelete.Sum(f => new FileInfo(f).Length);

            ConsoleHelper.WriteInfo();
            if (dryRunMode)
            {
                ConsoleHelper.WriteInfo("═══ DRY RUN MODE - No changes will be made ═══");
            }

            if (listMode)
            {
                string actionMsg = dryRunMode
                    ? $"Found {filesToDelete.Count} unused file(s) that WOULD be processed:"
                    : unsafeMode
                        ? $"The following {filesToDelete.Count} file(s) will be DELETED:"
                        : $"The following {filesToDelete.Count} file(s) will be MOVED to 'Unused Wavs':";
                
                if (unsafeMode && !dryRunMode)
                    ConsoleHelper.WriteWarning(actionMsg);
                else if (dryRunMode)
                    ConsoleHelper.WriteWarning(actionMsg);
                else
                    ConsoleHelper.WriteWarning(actionMsg);

                ConsoleHelper.WriteInfo();
                foreach (var file in filesToDelete)
                {
                    string relativePath = Path.GetRelativePath(rppDir, file);
                    long fileSize = new FileInfo(file).Length;
                    
                    if (unsafeMode && !dryRunMode)
                        ConsoleHelper.WriteInfo ($"  ✗ {relativePath} ({ConsoleHelper.FormatFileSize(fileSize)})");
                    else
                        ConsoleHelper.WriteInfo($"  → {relativePath} ({ConsoleHelper.FormatFileSize(fileSize)})");
                }
                ConsoleHelper.WriteInfo();
                ConsoleHelper.WriteInfo($"Total space: {ConsoleHelper.FormatFileSize(totalSize)}");
                ConsoleHelper.WriteInfo();
            }
            else
            {
                ConsoleHelper.WriteInfo($"Found {filesToDelete.Count} unused WAV file(s) ({ConsoleHelper.FormatFileSize(totalSize)})");
            }

            // Exit early if dry-run mode
            if (dryRunMode)
            {
                ConsoleHelper.WriteInfo("═══ Dry run complete - No files were modified ═══");
                return;
            }

            if (!silentMode)
            {
                Console.Write("Do you want to proceed? (y/N): ");
                var response = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(response) || !(response.Trim().ToLower() == "y" || response.Trim().ToLower() == "yes"))
                {
                    ConsoleHelper.WriteWarning("Aborted. No files were deleted or moved.");
                    return;
                }
            }

            Console.WriteLine();
            ConsoleHelper.WriteSeparator('═', 60);
            int affectedCount = 0;
            long processedSize = 0;
            
            if (unsafeMode)
            {
                foreach (var wavFile in filesToDelete)
                {
                    try
                    {
                        long fileSize = new FileInfo(wavFile).Length;
                        File.Delete(wavFile);
                        string relativePath = Path.GetRelativePath(rppDir, wavFile);
                        ConsoleHelper.WriteInfo($"Deleted {relativePath} ({ConsoleHelper.FormatFileSize(fileSize)})");
                        affectedCount++;
                        processedSize += fileSize;
                    }
                    catch (Exception ex)
                    {
                        string relativePath = Path.GetRelativePath(rppDir, wavFile);
                        ConsoleHelper.WriteError($"✗ Failed to delete {relativePath}: {ex.Message}");
                    }
                }
                
                ConsoleHelper.WriteInfo();
                ConsoleHelper.WriteSeparator('═', 60);
                ConsoleHelper.WriteSuccess($"✓ Deleted {affectedCount} of {filesToDelete.Count} file(s)");
                ConsoleHelper.WriteSuccess($"✓ Space freed: {ConsoleHelper.FormatFileSize(processedSize)}");
                ConsoleHelper.WriteSeparator('═', 60);
                return;
            }

            foreach (var wavFile in filesToDelete)
            {
                try
                {
                    long fileSize = new FileInfo(wavFile).Length;
                    
                    // Determine the correct Unused Wavs folder based on original location
                    string wavFileDir = Path.GetDirectoryName(wavFile);
                    string unusedDir = Path.Combine(wavFileDir, "Unused Wavs");
                    
                    if (!Directory.Exists(unusedDir)) 
                        Directory.CreateDirectory(unusedDir);

                    string destPath = Path.Combine(unusedDir, Path.GetFileName(wavFile));
                    File.Move(wavFile, destPath, overwrite: true);
                    
                    // Show relative path for better readability
                    string relativePath = Path.GetRelativePath(rppDir, wavFile);
                    ConsoleHelper.WriteInfo($"→ Moved: {relativePath} ({ConsoleHelper.FormatFileSize(fileSize)})");
                    affectedCount++;
                    processedSize += fileSize;
                }
                catch (Exception ex)
                {
                    string relativePath = Path.GetRelativePath(rppDir, wavFile);
                    ConsoleHelper.WriteError($"✗ Failed to move {relativePath}: {ex.Message}");
                }
            }
            
            Console.WriteLine();
            ConsoleHelper.WriteSeparator('═', 60);
            ConsoleHelper.WriteSuccess($"✓ Moved {affectedCount} of {filesToDelete.Count} file(s) to 'Unused Wavs' folder(s)");
            ConsoleHelper.WriteSuccess($"✓ Space moved: {ConsoleHelper.FormatFileSize(processedSize)}");
            ConsoleHelper.WriteSeparator('═', 60);
        }

        private static bool IsInExcludedFolder(string filePath, string rppDir, HashSet<string> excludedFolders)
        {
            if (excludedFolders.Count == 0)
                return false;

            // Get relative path and check if any part of the path matches excluded folders
            string relativePath = Path.GetRelativePath(rppDir, filePath);
            string[] pathParts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return pathParts.Any(part => excludedFolders.Contains(part));
        }
    }
}
