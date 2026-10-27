namespace StellarAdmin.Scenarios.Specs;

// Finds .spec.yaml files; each runner keeps its own specs and passes their folder
public static class SpecFiles
{
    private const string Extension = ".spec.yaml";

    // Full paths, ordered
    public static IReadOnlyList<string> Find(string directory) =>
        [
            .. Directory
                .EnumerateFiles(directory, $"*{Extension}", SearchOption.AllDirectories)
                .Order(),
        ];

    // The file name without .spec.yaml, e.g. trips-index
    public static string Name(string path) => Path.GetFileName(path)[..^Extension.Length];
}
