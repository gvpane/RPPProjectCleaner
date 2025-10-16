using System.Text.RegularExpressions;

namespace CleanWavFiles
{
    static class RppCleaner
    {
        public static void Clean(string filePath, bool unsafeMode, bool listMode, bool silentMode, bool dryRunMode, bool includeAllRpp, HashSet<string> excludedFolders = null)
        {
            excludedFolders ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string rppDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(rppDir))
            {
                ConsoleHelper.WriteError($"Could not determine directory for: {filePath}");
                return;
            }

            // Check for multiple .rpp files in directory
            var allRppFiles = RppReferenceAggregator.FindAllRppFiles(filePath);
            bool useMultiRpp = false;

            if (allRppFiles.Count > 1)
            {
                // Prompt user or use flag
                if (includeAllRpp)
                {
                    useMultiRpp = true;
                }
                else if (!dryRunMode) // Don't prompt in dry-run, just show what would happen
                {
                    useMultiRpp = RppReferenceAggregator.PromptForMultiRppInclusion(allRppFiles, filePath, silentMode);
                }
                else
                {
                    // In dry-run, default to multi-rpp to show complete picture
                    useMultiRpp = true;
                }
            }

            HashSet<string> referencedWavPaths;
            List<RppReferenceAggregator.RppFileInfo> rppInfos = null;

            if (useMultiRpp && allRppFiles.Count > 1)
            {
                // Aggregate references from all .rpp files
                rppInfos = RppReferenceAggregator.GetAllRppFileInfo(allRppFiles);
                referencedWavPaths = RppReferenceAggregator.AggregateReferences(rppInfos);
                
                if (dryRunMode || listMode)
                {
                    RppReferenceAggregator.DisplayRppSummary(rppInfos, rppDir, showWarnings: !dryRunMode);
                }

                int totalRefs = rppInfos.Sum(r => r.WavCount);
                ConsoleHelper.WriteSuccess($"Combined: {referencedWavPaths.Count} unique WAV file(s) referenced across all projects");
            }
            else
            {
                // Single .rpp file mode
                string rppContent = File.ReadAllText(filePath);
                
                referencedWavPaths = Regex.Matches(rppContent, "FILE \"([^\"]*\\.wav)\"", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(m => m.Groups[1].Value.Replace('/', '\\'))
                    .Select(relativePath => Path.GetFullPath(Path.Combine(rppDir, relativePath)))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                ConsoleHelper.WriteInfo($"Found {referencedWavPaths.Count} referenced WAV file(s) in the project");
            }
            
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
                ConsoleHelper.WritePrompt("Do you want to proceed? (y/N): ");
                var response = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(response) || !(response.Trim().ToLower() == "y" || response.Trim().ToLower() == "yes"))
                {
                    ConsoleHelper.WriteWarning("⚠️  Aborted. No files were deleted or moved.");
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

            // Create single "Unused Wavs" folder in root directory
            string rootUnusedDir = Path.Combine(rppDir, "Unused Wavs");
            if (!Directory.Exists(rootUnusedDir))
                Directory.CreateDirectory(rootUnusedDir);

            foreach (var wavFile in filesToDelete)
            {
                try
                {
                    long fileSize = new FileInfo(wavFile).Length;
                    
                    // Preserve directory structure within "Unused Wavs" folder
                    string relativePath = Path.GetRelativePath(rppDir, wavFile);
                    string destPath = Path.Combine(rootUnusedDir, relativePath);
                    
                    // Create subdirectories if needed
                    string destDir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Move(wavFile, destPath, overwrite: true);
                    
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
            ConsoleHelper.WriteSuccess($"✓ Moved {affectedCount} of {filesToDelete.Count} file(s) to 'Unused Wavs' folder");
            ConsoleHelper.WriteSuccess($"✓ Space moved: {ConsoleHelper.FormatFileSize(processedSize)}");
            ConsoleHelper.WriteInfo($"   Location: {Path.Combine(rppDir, "Unused Wavs")}");
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
