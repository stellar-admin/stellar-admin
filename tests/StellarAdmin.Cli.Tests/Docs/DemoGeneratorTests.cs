using System.Text.RegularExpressions;
using StellarAdmin.Cli.Docs;
using TUnit.Assertions;
using TUnit.Core;

namespace StellarAdmin.Cli.Tests.Docs;

public partial class DemoGeneratorTests
{
    private static readonly string PagesFolder = Path.Combine(
        DemoGenerator.DocsSamplesContentRoot,
        "Pages"
    );

    [Test]
    public async Task FindDemoPartials_ForDocsSamples_FindsOnlyPartialsAPageShows()
    {
        // Arrange
        var shownPartials = FindPages().SelectMany(FindShownPartials).ToHashSet();

        // Act
        var demoPartials = DemoGenerator.FindDemoPartials();

        // Assert
        var unshownDemos = demoPartials
            .Select(demo => demo.Name)
            .Where(name => !shownPartials.Contains(name))
            .Distinct()
            .ToList();
        await Assert.That(unshownDemos).IsEmpty();
    }

    [Test]
    public async Task FindDemoPartials_ForDocsSamples_RendersLinkedDemosInTheLinkedLayout()
    {
        // Arrange
        var links = FindPages().SelectMany(FindDocsStaticLinks).ToList();

        // Act
        var demoPartials = DemoGenerator.FindDemoPartials();

        // Assert
        var mismatches = links
            .Where(link =>
                !demoPartials.Any(demo => demo.Name == link.Name && demo.Layout == link.Layout)
            )
            .Select(link => $"{link.Name} ({link.Layout ?? "no layout"})")
            .ToList();
        await Assert.That(links).IsNotEmpty();
        await Assert.That(mismatches).IsEmpty();
    }

    // The samples' pages: every view under Pages that is no partial
    private static IEnumerable<string> FindPages() =>
        Directory
            .EnumerateFiles(PagesFolder, "*.cshtml", SearchOption.AllDirectories)
            .Where(path => !Path.GetFileName(path).StartsWith('_'));

    // The partials a page renders, and the ones it opens through a DocsStatic link
    private static IEnumerable<string> FindShownPartials(string page)
    {
        var content = ReadWithoutComments(page);
        var folder = Path.GetRelativePath(PagesFolder, Path.GetDirectoryName(page)!)
            .Replace(Path.DirectorySeparatorChar, '/');

        foreach (Match match in PartialRegex().Matches(content))
        {
            var name = match.Groups["name"].Value;
            if (name.StartsWith('@'))
                continue;

            yield return name.StartsWith('/') || name.StartsWith("~/", StringComparison.Ordinal)
                ? Path.ChangeExtension(name.TrimStart('~')["/Pages/".Length..], null)
                : $"{folder}/{Path.ChangeExtension(name, null)}";
        }

        foreach (var link in FindDocsStaticLinks(page))
            yield return link.Name;
    }

    private static IEnumerable<DocsStaticLink> FindDocsStaticLinks(string page)
    {
        foreach (Match tag in DocsStaticLinkRegex().Matches(ReadWithoutComments(page)))
        {
            var layout = LayoutRegex().Match(tag.Value);
            yield return new DocsStaticLink(
                NameRegex().Match(tag.Value).Groups["name"].Value,
                layout.Success ? layout.Groups["layout"].Value : null
            );
        }
    }

    private static string ReadWithoutComments(string page) =>
        RazorCommentRegex().Replace(File.ReadAllText(page), string.Empty);

    [GeneratedRegex(@"@\*.*?\*@", RegexOptions.Singleline)]
    private static partial Regex RazorCommentRegex();

    [GeneratedRegex(@"<partial\s[^>]*\bname=""(?<name>[^""]+)""")]
    private static partial Regex PartialRegex();

    [GeneratedRegex(@"<[\w-]+\s[^>]*\basp-route-name=""[^""]+""[^>]*>")]
    private static partial Regex DocsStaticLinkRegex();

    [GeneratedRegex(@"\basp-route-name=""(?<name>[^""]+)""")]
    private static partial Regex NameRegex();

    [GeneratedRegex(@"\basp-route-layout=""(?<layout>[^""]+)""")]
    private static partial Regex LayoutRegex();

    private sealed record DocsStaticLink(string Name, string? Layout);
}
