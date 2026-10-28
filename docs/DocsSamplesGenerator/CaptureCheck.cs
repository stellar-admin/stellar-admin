using System.Text.RegularExpressions;
using StellarAdmin.ScenarioTesting.Runner;
using YamlDotNet.RepresentationModel;

namespace DocsSamplesGenerator;

// A <Capture> on a docs page, and the screenshot names it shows: its name, or name-<variant> for each variant
internal sealed record CapturePlaceholder(
    string Page,
    int Line,
    string Name,
    string? Todo,
    IReadOnlyList<string> Images
);

// A screenshot a capture spec saves
internal sealed record CaptureShot(string Spec, string Name);

// Matches the docs pages' <Capture> placeholders against the capture specs' screenshots. A placeholder image
// is open when no spec saves it; a shot is unused when no page shows it. {name}-dark is a placeholder's dark
// version, not a separate image.
internal sealed partial class CaptureCheck
{
    private CaptureCheck(
        IReadOnlyList<CapturePlaceholder> placeholders,
        IReadOnlyList<CaptureShot> shots
    )
    {
        var shotNames = shots.Select(shot => shot.Name).ToHashSet();
        var imageNames = placeholders.SelectMany(placeholder => placeholder.Images).ToHashSet();

        Open =
        [
            .. placeholders
                .Select(placeholder =>
                    (
                        Placeholder: placeholder,
                        Missing: placeholder
                            .Images.Where(image => !shotNames.Contains(image))
                            .ToList()
                    )
                )
                .Where(open => open.Missing.Count > 0),
        ];
        Unused =
        [
            .. shots.Where(shot =>
                !imageNames.Contains(shot.Name)
                && !(shot.Name.EndsWith("-dark") && imageNames.Contains(shot.Name[..^5]))
            ),
        ];
    }

    public IReadOnlyList<(CapturePlaceholder Placeholder, List<string> Missing)> Open { get; }

    public IReadOnlyList<CaptureShot> Unused { get; }

    public static CaptureCheck Run(string docsFolder, string specsFolder, string repoRootFolder) =>
        new(
            FindPlaceholders(docsFolder),
            [
                .. SpecFiles
                    .Find(specsFolder)
                    .SelectMany(file =>
                        FindShots(file)
                            .Select(name => new CaptureShot(
                                Path.GetRelativePath(repoRootFolder, file),
                                name
                            ))
                    ),
            ]
        );

    // The variant's part of an image name: "Wordmark only" is wordmark-only
    public static string VariantSlug(string variant) =>
        NonSlugCharacters().Replace(variant.ToLowerInvariant(), "-").Trim('-');

    private static List<CapturePlaceholder> FindPlaceholders(string docsFolder)
    {
        List<CapturePlaceholder> placeholders = [];
        foreach (
            var file in Directory
                .EnumerateFiles(docsFolder, "*.mdx", SearchOption.AllDirectories)
                .Order()
        )
        {
            var text = File.ReadAllText(file);
            foreach (Match tag in CaptureTag().Matches(text))
            {
                var name = Attribute(tag.Value, "name");
                if (name is null)
                    continue;

                var variants = VariantsAttribute().Match(tag.Value) is { Success: true } match
                    ? QuotedString()
                        .Matches(match.Groups[1].Value)
                        .Select(variant => $"{name}-{VariantSlug(variant.Groups[1].Value)}")
                        .ToList()
                    : [name];

                placeholders.Add(
                    new(
                        Path.GetRelativePath(docsFolder, file),
                        text[..tag.Index].Count(c => c == '\n') + 1,
                        name,
                        Attribute(tag.Value, "todo"),
                        variants
                    )
                );
            }
        }

        return placeholders;
    }

    // Every screenshot step's name, at any depth, so steps inside within are found too
    private static IEnumerable<string> FindShots(string specFile)
    {
        var stream = new YamlStream();
        using (var reader = File.OpenText(specFile))
        {
            stream.Load(reader);
        }

        return stream.Documents.SelectMany(document => Shots(document.RootNode));

        static IEnumerable<string> Shots(YamlNode node) =>
            node switch
            {
                YamlMappingNode mapping => mapping.Children.SelectMany(child =>
                    child.Key is YamlScalarNode { Value: "screenshot" }
                        ? child.Value switch
                        {
                            YamlScalarNode { Value: { } name } => [name],
                            YamlMappingNode screenshot
                                when screenshot.Children.TryGetValue(
                                    new YamlScalarNode("name"),
                                    out var name
                                ) && name is YamlScalarNode { Value: { } value } => [value],
                            _ => [],
                        }
                        : Shots(child.Value)
                ),
                YamlSequenceNode sequence => sequence.Children.SelectMany(Shots),
                _ => [],
            };
    }

    private static string? Attribute(string tag, string name) =>
        Regex.Match(tag, $@"\b{name}=""([^""]*)""") is { Success: true } match
            ? match.Groups[1].Value
            : null;

    [GeneratedRegex(@"<Capture\b[^>]*?/>", RegexOptions.Singleline)]
    private static partial Regex CaptureTag();

    [GeneratedRegex(@"\bvariants=\{\[(.*?)\]\}", RegexOptions.Singleline)]
    private static partial Regex VariantsAttribute();

    [GeneratedRegex(@"""([^""]*)""")]
    private static partial Regex QuotedString();

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonSlugCharacters();
}
