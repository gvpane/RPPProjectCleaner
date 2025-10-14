namespace CleanWavFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: CleanWavFiles <path-to-rpp-file> [--unsafe] [--list] [--silent] [--multi]");
                Console.WriteLine("  --unsafe  DELETE unused WAVs permanently (default: move to 'Unused Wavs' folder)");
                Console.WriteLine("  --list    Show list of files before taking action");
                Console.WriteLine("  --silent  Skip confirmation prompt");
                Console.WriteLine("  --multi   Process all .rpp files in directory tree");
                return;
            }

            string rppPath = args[0];

            // Parse option flags (can appear in any order after the path)
            bool unsafeMode = args.Skip(1).Contains("--unsafe", StringComparer.OrdinalIgnoreCase);
            bool listMode = args.Skip(1).Contains("--list", StringComparer.OrdinalIgnoreCase);
            bool silentMode = args.Skip(1).Contains("--silent", StringComparer.OrdinalIgnoreCase);
            bool multiMode = args.Skip(1).Contains("--multi", StringComparer.OrdinalIgnoreCase);

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
                    RppCleaner.Clean(file, unsafeMode, listMode, silentMode);
                }
                return;
            }

            // Handle single file
            if (!File.Exists(rppPath))
            {
                Console.WriteLine($"RPP file not found: {rppPath}");
                return;
            }

            RppCleaner.Clean(rppPath, unsafeMode, listMode, silentMode);
        }
    }
}
