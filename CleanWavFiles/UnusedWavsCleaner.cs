namespace CleanWavFiles
{
    static class UnusedWavsCleaner
    {
        public static void CleanupUnusedWavsFolders(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                Console.WriteLine($"Directory not found: {rootPath}");
                return;
            }

            Console.WriteLine($"Searching for 'Unused Wavs' folders in: {rootPath}");
            Console.WriteLine();

            // Find all "Unused Wavs" directories recursively
            var unusedWavsDirs = Directory.GetDirectories(rootPath, "Unused Wavs", SearchOption.AllDirectories).ToList();

            if (unusedWavsDirs.Count == 0)
            {
                Console.WriteLine("No 'Unused Wavs' folders found.");
                return;
            }

            Console.WriteLine($"Found {unusedWavsDirs.Count} 'Unused Wavs' folder(s):");
            foreach (var dir in unusedWavsDirs)
            {
                string parentDir = Path.GetDirectoryName(dir);
                Console.WriteLine($"  - {parentDir}");
            }
            Console.WriteLine();

            // Delete all found directories
            int deletedCount = 0;
            int failedCount = 0;

            foreach (var dir in unusedWavsDirs)
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                    string parentDir = Path.GetDirectoryName(dir);
                    Console.WriteLine($"Deleted: {parentDir}\\Unused Wavs");
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    string parentDir = Path.GetDirectoryName(dir);
                    Console.WriteLine($"Failed to delete {parentDir}\\Unused Wavs: {ex.Message}");
                    failedCount++;
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Cleanup complete: {deletedCount} folder(s) deleted, {failedCount} failed.");
        }
    }
}
