using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CleanWavFiles
{
    class Program
    {
        private static string rppPath;
        private static bool safeMode = false;
        private static bool listMode = false;
        private static bool silentMode = false;
        private static bool multiMode = false;
        private static string rppDir;
        private static string rppContent;

        static void CleanRppFile(string filePath)
        {
            string rppDir = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(rppDir))
            {
                Console.WriteLine($"Could not determine directory for: {filePath}");
                return;
            }

            string rppContent = File.ReadAllText(filePath);
            var referencedWavs = Regex.Matches(rppContent, "FILE \"([^\"]*\\.wav)\"", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => Path.GetFileName(m.Groups[1].Value).ToLowerInvariant())
                .ToHashSet();

            bool listMode = Program.listMode;
            bool safeMode = Program.safeMode;
            bool silentMode = Program.silentMode;

            Console.WriteLine($"Found {referencedWavs.Count} referenced WAV files in the project: {filePath}");
            if (listMode)
            {
                foreach (var refWav in referencedWavs.OrderBy(x => x))
                    Console.WriteLine($"  - {refWav}");
            }

            var wavFiles = Directory.GetFiles(rppDir, "*.wav", SearchOption.TopDirectoryOnly);
            Console.WriteLine($"Found {wavFiles.Length} WAV files in directory");

            var filesToDelete = wavFiles
                .Where(wavFile => !referencedWavs.Contains(Path.GetFileName(wavFile).ToLowerInvariant()))
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
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
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
                string unusedDir = Path.Combine(rppDir, "Unused Wavs");
                if (!Directory.Exists(unusedDir)) Directory.CreateDirectory(unusedDir);

                foreach (var wavFile in filesToDelete)
                {
                    try
                    {
                        string destPath = Path.Combine(unusedDir, Path.GetFileName(wavFile));
                        File.Move(wavFile, destPath, overwrite: true);
                        Console.WriteLine($"Moved: {Path.GetFileName(wavFile)}");
                        affectedCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to move {Path.GetFileName(wavFile)}: {ex.Message}");
                    }
                }
                Console.WriteLine($"Moved {affectedCount} unused files to '{unusedDir}'.");
                Console.WriteLine("Cleanup complete.");
                return;
            }

            foreach (var wavFile in filesToDelete)
            {
                try
                {
                    File.Delete(wavFile);
                    Console.WriteLine($"Deleted: {Path.GetFileName(wavFile)}");
                    affectedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {Path.GetFileName(wavFile)}: {ex.Message}");
                }
            }
            Console.WriteLine($"Deleted {affectedCount} unused files.");
            Console.WriteLine("Cleanup complete.");
        }

        static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: CleanWavFiles <path-to-rpp-file> [--safe]");
                Console.WriteLine("  --safe   Move unused WAVs to 'Unused Wavs' folder instead of deleting");
                return;
            }

            rppPath = args[0];

            // Option flags can appear in any order after the path
            safeMode = args.Skip(1).Contains("--safe", StringComparer.OrdinalIgnoreCase);
            listMode = args.Skip(1).Contains("--list", StringComparer.OrdinalIgnoreCase);
            silentMode = args.Skip(1).Contains("--silent", StringComparer.OrdinalIgnoreCase);
            multiMode = args.Skip(1).Contains("--multi", StringComparer.OrdinalIgnoreCase);

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
                    Console.WriteLine($"Processing: {file}");
                    CleanRppFile(file);
                }
                return;
            }

            if (!File.Exists(rppPath))
            {
                Console.WriteLine($"RPP file not found: {rppPath}");
                return;
            }

            CleanRppFile(rppPath);
        }
    }
}
