namespace StellarAdmin.Cli;

// The product checkout the CLI was built in
internal static class RepoPaths
{
    public static readonly string Root = GetRoot();

    // Walks up from the build output, since CI's deterministic builds can rewrite compile-time source paths
    // to /_/, which rules out [CallerFilePath]
    private static string GetRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (File.Exists(Path.Combine(directory.FullName, "StellarAdmin.slnx")))
                return directory.FullName;
        }

        throw new DirectoryNotFoundException("Run the CLI from a product checkout's build output.");
    }
}
