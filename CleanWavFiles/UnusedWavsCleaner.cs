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
                ConsoleHelper.WriteSuccess("✓ No 'Unused Wavs' folders found.");
                return;
            }

            ConsoleHelper.WriteWarning($"Found {unusedWavsDirs.Count} 'Unused Wavs' folder(s):");
            ConsoleHelper.WriteInfo();
            foreach (var dir in unusedWavsDirs)
            {
                string parentDir = Path.GetDirectoryName(dir);
                ConsoleHelper.WriteWarning($"  • {parentDir}");
            }
            ConsoleHelper.WriteInfo();

            // Delete all found directories
            int deletedCount = 0;
            int failedCount = 0;

            ConsoleHelper.WriteSeparator('═', 60);
            
            foreach (var dir in unusedWavsDirs)
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                    string parentDir = Path.GetDirectoryName(dir);
                    string relativePath = Path.GetRelativePath(rootPath, dir);
                    ConsoleHelper.WriteSuccess($"✓ Deleted: {relativePath}");
                    deletedCount++;
                }
                catch (Exception ex)
                {
                    string parentDir = Path.GetDirectoryName(dir);
                    string relativePath = Path.GetRelativePath(rootPath, dir);
                    ConsoleHelper.WriteError($"✗ Failed: {relativePath} - {ex.Message}");
                    failedCount++;
                }
            }

            ConsoleHelper.WriteInfo();
            ConsoleHelper.WriteSeparator('═', 60);
            ConsoleHelper.WriteSuccess($"✓ Cleanup complete: {deletedCount} of {unusedWavsDirs.Count} folder(s) deleted");
            if (failedCount > 0)
            {
                ConsoleHelper.WriteError($"✗ {failedCount} folder(s) failed to delete");
            }
            ConsoleHelper.WriteSeparator('═', 60);
        }
    }
}
