// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ThemeGenerator;

public partial class Program
{
    /// <summary>
    ///     Tokens whose value is a marker class rather than a utility, so they cannot be expressed
    ///     as an <c>@apply</c> rule. <c>sa-menu-inverted</c> resolves to the literal <c>dark</c>
    ///     class, which must be present on the element itself for descendant <c>dark:</c> variants
    ///     and the <c>.dark</c> variable scope to take effect — the tag helper emits it directly.
    /// </summary>
    private static readonly string[] MarkerClassTokens = ["sa-menu-inverted"];

    private static readonly string CustomCssFolder = GetCustomCssFolder();

    // The per-theme custom CSS files sit in Themes/ alongside this source file. [CallerFilePath] is
    // resolved at compile time, so they are read straight from the source tree rather than copied
    // to the output.
    private static string GetCustomCssFolder([CallerFilePath] string sourceFilePath = "") =>
        Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "Themes");

    /// <summary>
    ///     Reads <c>Themes/&lt;Theme&gt;.custom.css</c> — hand-authored rules for styling a specific
    ///     theme beyond what the derived <c>--sa-gap-*</c> variables can express. Usually empty; the
    ///     content is copied verbatim into the generated theme file. Every theme must have one, so
    ///     adding a theme forces creating its file.
    /// </summary>
    private static Task<string> ReadCustomThemeCss(string themeName)
    {
        var customCssFile = Path.Combine(CustomCssFolder, $"{themeName}.custom.css");

        if (!File.Exists(customCssFile))
        {
            throw new FileNotFoundException(
                $"No custom CSS file for {themeName}. Every theme needs one (it may be empty).",
                customCssFile
            );
        }

        return File.ReadAllTextAsync(customCssFile);
    }

    public static async Task Main(string[] args)
    {
        var themesFolder = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "src",
                "StellarAdmin.TagHelpers",
                "Client",
                "css",
                "themes"
            )
        );
        string[] themeFiles =
        [
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-luma.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-lyra.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-maia.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-mira.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-nova.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-rhea.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-sera.css",
            "https://raw.githubusercontent.com/shadcn-ui/ui/refs/heads/main/apps/v4/registry/styles/style-vega.css",
        ];

        var httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

        foreach (var themeFile in themeFiles)
        {
            var responseMessage = await httpClient.GetAsync(themeFile);

            responseMessage.EnsureSuccessStatusCode();

            var responseText = await responseMessage.Content.ReadAsStringAsync();
            var themeTokens = ExtractComponentsFromThemeStyle(responseText)
                .DropReactAriaStyles()
                .AddFieldRadioGroupSupport()
                .RemoveAriaInvalidRing()
                .CreateInputValidationErrorClassesFromAriaInvalid()
                .ReplaceDuiCheckboxDataChecked()
                .ReplaceDuiRadioGroupItemDataChecked()
                .CleanSwitchClasses()
                .CleanToggleClasses()
                .CreateRadioButtonStyles()
                .CreateMenuSurfaceStyles()
                .CleanDialogClasses()
                .CleanPopoverClasses()
                .CleanSheetClasses()
                .CleanTooltipClasses();

            var lowerThemeName = Path.GetFileNameWithoutExtension(themeFile)
                .Replace("style-", string.Empty);
            var themeName = char.ToUpperInvariant(lowerThemeName[0]) + lowerThemeName[1..];

            Directory.CreateDirectory(themesFolder);
            await File.WriteAllTextAsync(
                Path.Combine(themesFolder, lowerThemeName + ".css"),
                BuildThemeCss(
                    themeTokens,
                    themeTokens.ExtractSpacingVariables(),
                    await ReadCustomThemeCss(themeName),
                    themeName,
                    Path.GetFileName(themeFile)
                )
            );
        }
    }

    /// <summary>
    ///     Emits every token as a <c>.sa-*</c> rule in the <c>components</c> layer, so an
    ///     author's utility classes out-rank them via layer order. Rules are written in extraction
    ///     order — base tokens precede their <c>-variant-*</c>/<c>-size-*</c> siblings — so
    ///     same-specificity conflicts resolve to the later (more specific) rule by source order.
    ///     <para>
    ///         The derived spacing variables lead the file as a <c>:root</c> block inside the same
    ///         layer, so an app's own unlayered <c>:root</c> declarations override them — the same
    ///         customization model as the palette variables.
    ///     </para>
    ///     <para>
    ///         Hand-authored rules from <c>Themes/&lt;Theme&gt;.custom.css</c> close the file,
    ///         copied verbatim but still inside the layer, so bare <c>.sa-*</c> rules written there
    ///         get theme-rule precedence automatically (losing to structural components.css
    ///         declarations and author utilities, like every other themed rule).
    ///     </para>
    /// </summary>
    private static string BuildThemeCss(
        Dictionary<string, string> themeTokens,
        Dictionary<string, string> spacingVariables,
        string customCss,
        string themeName,
        string sourceFileName
    )
    {
        var css = new StringBuilder();

        css.Append(
            $"/* Generated by util/ThemeGenerator from shadcn {sourceFileName} — do not edit. */\n"
        );
        css.Append("@layer components.theme {\n");

        css.Append("  :root {\n");
        foreach (var (name, value) in spacingVariables)
        {
            css.Append($"    {name}: {value};\n");
        }
        css.Append("  }\n");

        foreach (var (name, value) in themeTokens)
        {
            if (MarkerClassTokens.Contains(name))
            {
                continue;
            }

            var classes = string.Join(' ', value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

            css.Append($"  .{name} {{\n");
            if (classes.Length > 0)
            {
                css.Append($"    @apply {classes};\n");
            }
            css.Append("  }\n");
        }

        if (!string.IsNullOrWhiteSpace(customCss))
        {
            css.Append($"\n  /* From Themes/{themeName}.custom.css */\n");
            css.Append(customCss.Trim());
            css.Append('\n');
        }

        css.Append("}\n");

        return css.ToString();
    }

    [GeneratedRegex(@".cn-(?<name>\S+).*{\n(\s)*@apply(\s)+(?<value>.*);")]
    public static partial Regex StyleRegex();

    private static Dictionary<string, string> ExtractComponentsFromThemeStyle(string input)
    {
        var tokens = new Dictionary<string, string>();

        foreach (Match match in StyleRegex().Matches(input))
        {
            tokens.Add($"sa-{match.Groups["name"].Value}", match.Groups["value"].Value);
        }

        return tokens;
    }
}
