using System.Text.RegularExpressions;
using StellarAdmin.ScenarioTesting.Hosting;
using StellarAdmin.ScenarioTesting.Runner;
using YamlDotNet.RepresentationModel;

namespace StellarAdmin.Cli.Docs;

// Checks that each docs screenshot is named for the docs page it belongs to: a scenario, spec or screenshot in
// Screenshots/<Page>/ starts with the page's name, such as scopes-tabs in Scopes/ for scopes.mdx, and a
// <Screenshot>'s images start with the name of its page, or of another page that shows it too. A page goes by its
// file name or by its folder's name in front of it, and an index.mdx by its folder's name too, so
// tables/sorting-and-paging.mdx is tables-sorting-and-paging and tables/index.mdx is tables
internal static partial class ScreenshotNames
{
    // A description of each name that breaks the rule
    public static List<string> Check(
        string docsFolder,
        string specsFolder,
        string repoRootFolder,
        IEnumerable<Scenario> scenarios
    )
    {
        var pageNames = PageNames(docsFolder);
        var pages = pageNames.Values.SelectMany(names => names).ToHashSet();
        List<string> problems = [];

        foreach (var scenario in scenarios)
        {
            var type = scenario.GetType();
            var folder = type.Namespace?.Split('.') is [.., "Screenshots", var last] ? last : null;
            CheckName(problems, pages, folder, scenario.Name, $"Scenario {type.Name}");
        }

        foreach (var file in SpecFiles.Find(specsFolder))
        {
            var spec = Path.GetRelativePath(repoRootFolder, file);
            var folder = Path.GetDirectoryName(Path.GetRelativePath(specsFolder, file))
                is { Length: > 0 } directory
                ? directory
                : null;
            CheckName(problems, pages, folder, SpecFiles.Name(file), $"Spec {spec}");
            if (ScenarioName(file) is { } scenario)
                CheckName(problems, pages, folder, scenario, $"Scenario {scenario} in {spec}");

            foreach (var shot in ScreenshotCheck.FindShots(file))
                CheckName(problems, pages, folder, shot, $"Screenshot {shot} in {spec}");
        }

        var placeholders = ScreenshotCheck.FindPlaceholders(docsFolder);
        foreach (var placeholder in placeholders)
        {
            var shownOn = placeholders
                .Where(other => other.Name == placeholder.Name)
                .SelectMany(other =>
                    pageNames[Path.GetFullPath(Path.Combine(docsFolder, other.Page))]
                );
            if (
                !shownOn.Any(page =>
                    placeholder.Images.All(image =>
                        image.StartsWith(page + "-", StringComparison.Ordinal)
                    )
                )
            )
            {
                problems.Add(
                    $"Screenshot {placeholder.Name} ({placeholder.Page}:{placeholder.Line}) has images that don't start with the name of a page that shows it"
                );
            }
        }

        return problems;
    }

    // The names each docs page goes by, by its full path
    private static Dictionary<string, string[]> PageNames(string docsFolder) =>
        Directory
            .EnumerateFiles(docsFolder, "*.mdx", SearchOption.AllDirectories)
            .Select(Path.GetFullPath)
            .ToDictionary(file => file, Names);

    // A page's file name and its folder's name in front of it, and an index.mdx's folder name
    private static string[] Names(string file)
    {
        var name = Path.GetFileNameWithoutExtension(file);
        var folder = Path.GetFileName(Path.GetDirectoryName(file))!;

        return name is "index" ? [name, folder] : [name, $"{folder}-{name}"];
    }

    // Checks name starts with the page of its folder, such as index-pages- in IndexPages/
    private static void CheckName(
        List<string> problems,
        HashSet<string> pages,
        string? folder,
        string name,
        string what
    )
    {
        if (folder is null)
        {
            problems.Add($"{what} isn't in a docs page's folder under Screenshots/");
            return;
        }

        var page = PageName(folder);
        if (!pages.Contains(page))
            problems.Add($"{what} is in {folder}/, but there's no docs page {page}.mdx");
        else if (!name.StartsWith(page + "-", StringComparison.Ordinal))
            problems.Add($"{what} doesn't start with {page}-, the page of its folder {folder}/");
    }

    // The spec's scenario: value
    private static string? ScenarioName(string specFile)
    {
        var stream = new YamlStream();
        using (var reader = File.OpenText(specFile))
        {
            stream.Load(reader);
        }

        return
            stream.Documents.FirstOrDefault()?.RootNode is YamlMappingNode root
            && root.Children.TryGetValue(new YamlScalarNode("scenario"), out var scenario)
            && scenario is YamlScalarNode { Value: { } value }
            ? value
            : null;
    }

    // The docs page a folder holds the screenshots of: SortingAndPaging is sorting-and-paging, DataFromAHandler is
    // data-from-a-handler
    private static string PageName(string folder) =>
        WordBoundary().Replace(folder, "-").ToLowerInvariant();

    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex WordBoundary();
}
