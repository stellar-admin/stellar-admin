using System.Text.Json;
using System.Text.RegularExpressions;

namespace DocsSamples;

/// <summary>A theme the samples' theme pickers offer: the default, or a theme fixture.</summary>
/// <param name="Name">The file name without extension, or <c>default</c>.</param>
/// <param name="Label">The first line of the fixture's header comment.</param>
/// <param name="Description">The paragraph under that line.</param>
/// <param name="GoogleFonts">The Google Fonts family parameter for the theme's <c>--sa-font-sans</c>, if it is a web font.</param>
public sealed record ThemeFixture(
    string Name,
    string Label,
    string Description,
    string? GoogleFonts
);

/// <summary>
/// Reads the theme fixtures in util/theme-check/presets, the one source of the themes the samples'
/// pickers, the docs export and the website's theme builder offer. DocsSamples, ComponentPlayground
/// and the DocsSamplesGenerator share this file.
/// </summary>
public static partial class ThemeFixtures
{
    public const string DefaultName = "default";

    public static string Folder(string repoRoot) =>
        Path.Combine(repoRoot, "util", "theme-check", "presets");

    /// <summary>The default theme first, then each fixture by name.</summary>
    public static IReadOnlyList<ThemeFixture> Load(string repoRoot)
    {
        var fonts = ReadFontChoices(repoRoot, out var defaultFont);
        var themes = new List<ThemeFixture>
        {
            new(
                DefaultName,
                "Default",
                "The library's default theme: stellar-admin.css alone.",
                fonts.GetValueOrDefault(defaultFont)
            ),
        };

        foreach (var file in Directory.EnumerateFiles(Folder(repoRoot), "*.css").Order())
        {
            var css = File.ReadAllText(file);
            var header = HeaderRegex().Match(css);
            if (!header.Success)
                throw new InvalidOperationException(
                    $"{file}: the header comment must start with \"Theme fixture: <label>\"."
                );

            var font = FontSansRegex().Match(css);
            themes.Add(
                new(
                    Path.GetFileNameWithoutExtension(file),
                    header.Groups["label"].Value.Trim(),
                    Normalize(header.Groups["description"].Value),
                    fonts.GetValueOrDefault(
                        font.Success ? Normalize(font.Groups[1].Value) : defaultFont
                    )
                )
            );
        }

        return themes;
    }

    // The manifest's --sa-font-sans choices, from a font stack to its Google Fonts family.
    private static Dictionary<string, string> ReadFontChoices(
        string repoRoot,
        out string defaultFont
    )
    {
        var manifest = Path.Combine(
            repoRoot,
            "src",
            "StellarAdmin.TagHelpers",
            "Client",
            "css",
            "knobs.json"
        );
        using var document = JsonDocument.Parse(File.ReadAllText(manifest));
        var knob = document
            .RootElement.GetProperty("knobs")
            .EnumerateArray()
            .Single(k => k.GetProperty("name").GetString() == "--sa-font-sans");

        defaultFont = Normalize(knob.GetProperty("default").GetString()!);
        var fonts = new Dictionary<string, string>();
        foreach (var choice in knob.GetProperty("choices").EnumerateArray())
        {
            if (choice.TryGetProperty("google", out var google))
                fonts[Normalize(choice.GetProperty("value").GetString()!)] = google.GetString()!;
        }

        return fonts;
    }

    private static string Normalize(string value) => WhitespaceRegex().Replace(value, " ").Trim();

    [GeneratedRegex(
        @"\A\s*/\*\s*Theme fixture:\s*(?<label>[^\r\n]+)\r?\n(?<description>(?:[ \t]*\S[^\r\n]*\r?\n)+)"
    )]
    private static partial Regex HeaderRegex();

    [GeneratedRegex(@"--sa-font-sans\s*:\s*([^;]+);")]
    private static partial Regex FontSansRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
