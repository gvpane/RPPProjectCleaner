namespace CleanWavFiles
{
    static class UnusedWavsCleaner
    {
        public static void CleanupUnusedWavsFolders(string rootPath)
        {
            if (!Directory.Exists(rootPath))
            {
                ConsoleHelper.WriteError($"Directory not found: {rootPath}");
                return;
            }

            ConsoleHelper.WriteInfo($"Searching for 'Unused Wavs' folders in: {rootPath}");
            ConsoleHelper.WriteInfo();

            // Find all "Unused Wavs" directories recursively
            var unusedWavsDirs = Directory.GetDirectories(rootPath, "Unused Wavs", SearchOption.AllDirectories).ToList();

            if (unusedWavsDirs.Count == 0)
            {
                ConsoleHelper.WriteWarning("No 'Unused Wavs' folders found.");
                return;
            }

            ConsoleHelper.WriteInfo($"Found {unusedWavsDirs.Count} 'Unused Wavs' folder(s):");
            foreach (var dir in unusedWavsDirs)
            {
                string parentDir = Path.GetDirectoryName(dir);
                ConsoleHelper.WriteInfo($"  - {parentDir}");
            }
            ConsoleHelper.WriteInfo();

            // Delete all found directories
            int deletedCount = 0;
            int failedCount = 0;

            foreach (var dir in unusedWavsDirs)
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                    string parentDir = Path.GetDirectoryName(dir);
                    Console.WriteLine($"Cleaned {parentDir}");
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    string parentDir = Path.GetDirectoryName(dir);
                    ConsoleHelper.WriteError($"Failed to delete {parentDir}: {ex.Message}");
                    failedCount++;
                }
            }

            ConsoleHelper.WriteInfo();
            ConsoleHelper.WriteSuccess($"Cleanup complete: {deletedCount} folder(s) deleted, {failedCount} failed.");
        }
    }
}
