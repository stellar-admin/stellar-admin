using System.Runtime.CompilerServices;

namespace StellarAdmin.Cli.Docs;

// The repository and the website checkout the docs commands write into
internal static class DocsPaths
{
    public static readonly string RepoRoot = GetRepoRoot();

    // The website checkout: STELLARADMIN_WEBSITE_DIR, or the sibling website folder
    public static readonly string WebsiteRoot = Environment.GetEnvironmentVariable(
        "STELLARADMIN_WEBSITE_DIR"
    )
        is { Length: > 0 } websiteDir
        ? Path.GetFullPath(websiteDir)
        : Path.GetFullPath(Path.Combine(RepoRoot, "..", "website"));

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

    // DocsPaths.cs lives at <repoRoot>/tools/StellarAdmin.Cli/Docs/, so the repo root is three directories
    // up from this source file. [CallerFilePath] is resolved at compile time, which keeps this correct
    // regardless of the working directory the CLI is launched from.
    private static string GetRepoRoot([CallerFilePath] string sourceFilePath = "") =>
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "..", "..", ".."));
}
