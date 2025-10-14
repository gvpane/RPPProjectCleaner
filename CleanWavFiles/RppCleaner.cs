using System.Text.RegularExpressions;

namespace CleanWavFiles
{
    static class RppCleaner
    {
        public static void Clean(string filePath, bool safeMode, bool listMode, bool silentMode)
        {
            string rppDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(rppDir))
            {
                Console.WriteLine($"Could not determine directory for: {filePath}");
                return;
            }

            string rppContent = File.ReadAllText(filePath);
            
            // Extract relative paths from .rpp and convert to absolute paths
            var referencedWavPaths = Regex.Matches(rppContent, "FILE \"([^\"]*\\.wav)\"", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Replace('/', '\\'))  // Normalize path separators
                .Select(relativePath => Path.GetFullPath(Path.Combine(rppDir, relativePath)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            Console.WriteLine($"Found {referencedWavPaths.Count} referenced WAV files in the project: {filePath}");
            
            if (listMode)
            {
                foreach (var refWav in referencedWavPaths.OrderBy(x => x))
                    Console.WriteLine($"  - {refWav}");
            }

            // Collect WAV files from both root directory and Media subfolder
            var wavFiles = Directory.GetFiles(rppDir, "*.wav", SearchOption.TopDirectoryOnly).ToList();
            string mediaDir = Path.Combine(rppDir, "Media");
            if (Directory.Exists(mediaDir))
            {
                wavFiles.AddRange(Directory.GetFiles(mediaDir, "*.wav", SearchOption.TopDirectoryOnly));
            }
            Console.WriteLine($"Found {wavFiles.Count} WAV files in directory (including Media folder)");

            // Compare full absolute paths
            var filesToDelete = wavFiles
                .Where(wavFile => !referencedWavPaths.Contains(Path.GetFullPath(wavFile)))
                .ToList();

            if (filesToDelete.Count == 0)
            {
                Console.WriteLine("No unused .wav files to delete. Exiting.");
                return;
            }

            if (listMode)
            {
                string actionMsg = safeMode
                    ? "The following files are NOT referenced in the project and will be MOVED to 'Unused Wavs':"
                    : "The following files are NOT referenced in the project and will be DELETED:";
                Console.WriteLine(actionMsg);
                foreach (var file in filesToDelete)
                {
                    string relativePath = Path.GetRelativePath(rppDir, file);
                    Console.WriteLine($"  - {relativePath}");
                }
            }

            if (!silentMode)
            {
                Console.Write("Do you want to proceed? (y/N): ");
                var response = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(response) || !(response.Trim().ToLower() == "y" || response.Trim().ToLower() == "yes"))
                {
                    Console.WriteLine("Aborted. No files were deleted or moved.");
                    return;
                }
            }

            int affectedCount = 0;
            if (safeMode)
            {
                foreach (var wavFile in filesToDelete)
                {
                    try
                    {
                        // Determine the correct Unused Wavs folder based on original location
                        string wavFileDir = Path.GetDirectoryName(wavFile);
                        string unusedDir = Path.Combine(wavFileDir, "Unused Wavs");
                        
                        if (!Directory.Exists(unusedDir)) 
                            Directory.CreateDirectory(unusedDir);

                        string destPath = Path.Combine(unusedDir, Path.GetFileName(wavFile));
                        File.Move(wavFile, destPath, overwrite: true);
                        
                        // Show relative path for better readability
                        string relativePath = Path.GetRelativePath(rppDir, wavFile);
                        Console.WriteLine($"Moved: {relativePath}");
                        affectedCount++;
                    }
                    catch (Exception ex)
                    {
                        string relativePath = Path.GetRelativePath(rppDir, wavFile);
                        Console.WriteLine($"Failed to move {relativePath}: {ex.Message}");
                    }
                }
                Console.WriteLine($"Moved {affectedCount} unused files to 'Unused Wavs' folder(s).");
                Console.WriteLine("Cleanup complete.");
                return;
            }

            foreach (var wavFile in filesToDelete)
            {
                try
                {
                    File.Delete(wavFile);
                    string relativePath = Path.GetRelativePath(rppDir, wavFile);
                    Console.WriteLine($"Deleted: {relativePath}");
                    affectedCount++;
                }
                catch (Exception ex)
                {
                    string relativePath = Path.GetRelativePath(rppDir, wavFile);
                    Console.WriteLine($"Failed to delete {relativePath}: {ex.Message}");
                }
            }
            Console.WriteLine($"Deleted {affectedCount} unused files.");
            Console.WriteLine("Cleanup complete.");
        }
    }
}
