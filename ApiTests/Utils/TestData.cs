namespace ApiTests.Utils
{
    public static class TestData
    {
        private static string? FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                if (dir.GetFiles("*.sln").Length > 0) return dir.FullName;
                if (dir.GetDirectories("testdata").Length > 0) return dir.FullName;
                dir = dir.Parent;
            }
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        }

        public static string GetFileContents(string relativePath)
        {
            var repoRoot = FindRepoRoot() ?? throw new InvalidOperationException("Could not determine repository root for testdata");
            var full = Path.Combine(repoRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(full)) throw new FileNotFoundException($"Test data file not found: {full}");
            return File.ReadAllText(full);
        }

        public static T GetFileContentsAs<T>(string relativePath)
        {
            var json = GetFileContents(relativePath);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json)!;
        }
    }
}
