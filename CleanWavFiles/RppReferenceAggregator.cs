using System.Text.RegularExpressions;

namespace CleanWavFiles
{
    static class RppReferenceAggregator
    {
        public class RppFileInfo
        {
            public string FilePath { get; set; }
            public int WavCount { get; set; }
            public int MissingWavCount { get; set; }
            public HashSet<string> ReferencedWavs { get; set; }
        }

        public static List<string> FindAllRppFiles(string rppPath)
        {
            string rppDir = Path.GetDirectoryName(rppPath);
            if (string.IsNullOrEmpty(rppDir))
                return new List<string> { rppPath };

            // Find all .rpp files in the same directory and subdirectories
            return Directory.GetFiles(rppDir, "*.rpp", SearchOption.AllDirectories)
                .OrderBy(f => f)
                .ToList();
        }

        public static HashSet<string> ExtractWavReferences(string rppFilePath)
        {
            string rppDir = Path.GetDirectoryName(rppFilePath);
            if (string.IsNullOrEmpty(rppDir))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            string rppContent = File.ReadAllText(rppFilePath);
            
            // Extract relative paths from .rpp and convert to absolute paths
            return Regex.Matches(rppContent, "FILE \"([^\"]*\\.wav)\"", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Groups[1].Value.Replace('/', '\\'))
                .Select(relativePath => Path.GetFullPath(Path.Combine(rppDir, relativePath)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public static List<RppFileInfo> GetAllRppFileInfo(List<string> rppFiles)
        {
            var results = new List<RppFileInfo>();

            foreach (var rppFile in rppFiles)
            {
                try
                {
                    var referencedWavs = ExtractWavReferences(rppFile);
                    int missingCount = referencedWavs.Count(wav => !File.Exists(wav));

                    results.Add(new RppFileInfo
                    {
                        FilePath = rppFile,
                        WavCount = referencedWavs.Count,
                        MissingWavCount = missingCount,
                        ReferencedWavs = referencedWavs
                    });
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"Failed to parse {Path.GetFileName(rppFile)}: {ex.Message}");
                }
            }

            return results;
        }

        public static HashSet<string> AggregateReferences(List<RppFileInfo> rppInfos)
        {
            var allReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var info in rppInfos)
            {
                allReferences.UnionWith(info.ReferencedWavs);
            }

            return allReferences;
        }

        public static bool PromptForMultiRppInclusion(List<string> rppFiles, string currentRppPath, bool silentMode)
        {
            if (rppFiles.Count <= 1)
                return false; // Only one file, no need to prompt

            if (silentMode)
                return true; // Silent mode defaults to YES

            ConsoleHelper.WriteInfo();
            ConsoleHelper.WriteWarning($"⚠️  Found {rppFiles.Count} .rpp file(s) in this directory:");
            ConsoleHelper.WriteInfo();
            
            string rppDir = Path.GetDirectoryName(currentRppPath);
            foreach (var rppFile in rppFiles.Take(10)) // Show max 10
            {
                string relativePath = Path.GetRelativePath(rppDir, rppFile);
                ConsoleHelper.WriteWarning($"  • {relativePath}");
            }
            
            if (rppFiles.Count > 10)
            {
                ConsoleHelper.WriteInfo($"  ... and {rppFiles.Count - 10} more");
            }

            ConsoleHelper.WriteInfo();
            ConsoleHelper.WriteInfo("Include all .rpp files when checking references? (Recommended)");
            ConsoleHelper.WriteInfo("This ensures WAVs used in any version won't be removed.");
            ConsoleHelper.WriteInfo();
            ConsoleHelper.WritePrompt("Include all .rpp files? [Y/n]: ");
            
            var response = Console.ReadLine()?.Trim().ToLower();
            
            // Default to YES (empty input or 'y')
            return string.IsNullOrEmpty(response) || response == "y" || response == "yes";
        }

        public static void DisplayRppSummary(List<RppFileInfo> rppInfos, string baseDir, bool showWarnings = true)
        {
            ConsoleHelper.WriteInfo();
            ConsoleHelper.WriteInfo($"Scanned {rppInfos.Count} .rpp file(s):");
            ConsoleHelper.WriteInfo();

            foreach (var info in rppInfos)
            {
                string relativePath = Path.GetRelativePath(baseDir, info.FilePath);
                
                if (info.MissingWavCount > 0 && showWarnings)
                {
                    ConsoleHelper.WriteWarning($"  • {relativePath} ({info.WavCount} WAV refs, {info.MissingWavCount} missing)");
                }
                else
                {
                    ConsoleHelper.WriteSuccess($"  • {relativePath} ({info.WavCount} WAV refs)");
                }
            }

            // Show warnings for orphaned .rpp files
            if (showWarnings)
            {
                var orphanedRpps = rppInfos.Where(r => r.MissingWavCount > 0).ToList();
                if (orphanedRpps.Any())
                {
                    ConsoleHelper.WriteInfo();
                    ConsoleHelper.WriteWarning($"⚠️  {orphanedRpps.Count} project(s) reference missing WAV files:");
                    foreach (var rpp in orphanedRpps)
                    {
                        string relativePath = Path.GetRelativePath(baseDir, rpp.FilePath);
                        ConsoleHelper.WriteWarning($"   • {relativePath} (missing {rpp.MissingWavCount} file(s))");
                    }
                    ConsoleHelper.WriteInfo("   These might be old/broken project versions.");
                }
            }

            ConsoleHelper.WriteInfo();
        }
    }
}
