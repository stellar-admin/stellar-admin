namespace StellarAdmin.Cli.Docs;

// The website checkout the docs commands write into
internal static class DocsPaths
{
    // The website checkout: STELLARADMIN_WEBSITE_DIR, or the sibling website folder
    public static readonly string WebsiteRoot = Environment.GetEnvironmentVariable(
        "STELLARADMIN_WEBSITE_DIR"
    )
        is { Length: > 0 } websiteDir
        ? Path.GetFullPath(websiteDir)
        : Path.GetFullPath(Path.Combine(RepoPaths.Root, "..", "website"));

    // Deletes each folder or file that exists
    public static void Delete(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
            else if (File.Exists(path))
                File.Delete(path);
        }
    }
}
