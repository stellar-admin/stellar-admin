using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ThemePackGenerator;

public static partial class CustomThemeTokenExtensions
{
    private static readonly string TemplateFolder = GetTemplateFolder();

    // The templates sit in ThemePacks/ alongside this source file. [CallerFilePath] is resolved at
    // compile time, so they are read straight from the source tree rather than copied to the output.
    private static string GetTemplateFolder([CallerFilePath] string sourceFilePath = "") =>
        Path.Combine(Path.GetDirectoryName(sourceFilePath)!, "ThemePacks");

    /// <summary>
    ///     A token declaration in a template — a single leading dash followed by the token name. The
    ///     value on the next line may itself start with dashes (placeholders are written
    ///     <c>--sa-token-*</c>), which this deliberately does not match.
    /// </summary>
    [GeneratedRegex(@"^-(?<name>sa-\S+)\s*$")]
    public static partial Regex TemplateTokenDeclarationRegex();

    /// <summary>
    ///     A gap-N applied to the element itself. The lookbehind rejects variant-prefixed forms such as
    ///     has-[&gt;[data-slot=checkbox-group]]:gap-3, which sit in the same class string but describe a
    ///     nested case rather than the component's own spacing.
    /// </summary>
    [GeneratedRegex(@"(?<![-:\w])gap-(?<value>\d+(?:\.\d+)?)(?![\w.-])")]
    public static partial Regex StandaloneGapDetectionRegex();

    /// <summary>
    ///     The base --card-spacing declaration. The lookbehind rejects the data-[size=sm]: variant.
    /// </summary>
    [GeneratedRegex(@"(?<![-:\w])\[--card-spacing:--spacing\((?<value>\d+(?:\.\d+)?)\)\]")]
    public static partial Regex CardSpacingDetectionRegex();

    extension(Dictionary<string, string> tokens)
    {
        /// <summary>
        ///     Appends the tokens for StellarAdmin.UI's own components, which have no upstream
        ///     <c>.cn-*</c> block to extract.
        ///     <para>
        ///         The values come from the theme itself (see
        ///         <see cref="ExtractSpacingScaleVariables" />), but which component uses which step is
        ///         authored by hand in <c>ThemePacks/&lt;theme&gt;.themepack.template</c>. A template
        ///         references a step as <c>--sa-token-*</c> and this substitutes the theme's class for
        ///         it, so adding a theme-aware component is a template edit rather than a code change.
        ///     </para>
        /// </summary>
        public Dictionary<string, string> AppendCustomThemeTokens(string themeName)
        {
            var spacingScaleVariables = ExtractSpacingScaleVariables(tokens);
            var templateFile = Path.Combine(TemplateFolder, $"{themeName}.themepack.template");

            if (!File.Exists(templateFile))
            {
                throw new FileNotFoundException(
                    $"No theme pack template for {themeName}. Every theme needs one.",
                    templateFile
                );
            }

            // Longest name first, so a shorter one that prefixes a longer one cannot consume part of it.
            var variables = spacingScaleVariables
                .OrderByDescending(variable => variable.Key.Length)
                .ToArray();

            var output = new Dictionary<string, string>(tokens);

            foreach (var (name, value) in ParseTemplate(templateFile))
            {
                var classes = value;

                foreach (var (variable, replacement) in variables)
                {
                    classes = classes.Replace($"--{variable}", replacement);
                }

                if (classes.Contains("--sa-token-"))
                {
                    // ThemeManager cannot tell a bad value from a good one, so an unresolved placeholder
                    // would ship as a class name that silently matches nothing.
                    throw new InvalidOperationException(
                        $"{themeName}: token '{name}' references an unknown scale variable: {classes}"
                    );
                }

                output[name] = classes;
            }

            return output;
        }
    }

    /// <summary>The Tailwind spacing values a derived step is allowed to land on.</summary>
    private static readonly double[] SpacingLadder =
    [
        0.5,
        1,
        1.5,
        2,
        2.5,
        3,
        3.5,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        14,
        16,
    ];

    /// <summary>Tailwind's per-side and logical-side radius segments, which we never treat as the element's own radius.</summary>
    private static readonly string[] RadiusSides =
    [
        "t",
        "r",
        "b",
        "l",
        "s",
        "e",
        "ss",
        "se",
        "es",
        "ee",
        "tl",
        "tr",
        "bl",
        "br",
    ];

    /// <summary>
    ///     Reads a template using the same grammar as a generated theme pack: a declaration line names
    ///     the token, the line below it holds the value.
    /// </summary>
    private static IEnumerable<KeyValuePair<string, string>> ParseTemplate(string templateFile)
    {
        var lines = File.ReadAllLines(templateFile);

        for (var i = 0; i < lines.Length - 1; i++)
        {
            var match = TemplateTokenDeclarationRegex().Match(lines[i]);

            if (match.Success)
            {
                yield return new KeyValuePair<string, string>(
                    match.Groups["name"].Value,
                    lines[i + 1].Trim()
                );
            }
        }
    }

    private static string Format(double value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);

    private static double DetectCardSpacing(Dictionary<string, string> input)
    {
        if (input.TryGetValue("sa-card", out var classes))
        {
            var match = CardSpacingDetectionRegex().Match(classes);

            if (match.Success)
            {
                return double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
            }
        }

        throw new ArgumentException("Could not detect card spacing");
    }

    private static double DetectGap(Dictionary<string, string> input, string token)
    {
        if (input.TryGetValue(token, out var classes))
        {
            var match = StandaloneGapDetectionRegex().Match(classes);

            if (match.Success)
            {
                return double.Parse(match.Groups["value"].Value, CultureInfo.InvariantCulture);
            }
        }

        throw new ArgumentException("Could not detect gap", nameof(token));
    }

    /// <summary>
    ///     Derives a five-step spacing ladder for the layout primitives (sa-stack, sa-group,
    ///     sa-container), which are StellarAdmin.UI's own components and have no upstream
    ///     .cn-* block to extract.
    ///     <para>
    ///         Each shadcn theme encodes its density in places that move together, so rather than
    ///         inventing numbers we read the decisions the theme already made:
    ///         <list type="bullet">
    ///             <item>xs — sa-card-header gap: inside one labelled unit (title / description).</item>
    ///             <item>sm — sa-field gap: a control and its label or help text.</item>
    ///             <item>md — sa-field-set gap: sibling items in a list or form. The default.</item>
    ///             <item>lg — sa-field-group gap: distinct groups or sections.</item>
    ///             <item>xl — three ladder steps above lg; no upstream equivalent exists.</item>
    ///         </list>
    ///         The ladder is forced monotonic afterwards: in some themes sa-field-group ties
    ///         sa-field-set, which would leave lg and md indistinguishable.
    ///     </para>
    /// </summary>
    private static Dictionary<string, string> ExtractSpacingScaleVariables(
        Dictionary<string, string> input
    )
    {
        var scaleVariables = new Dictionary<string, string>();

        double[] values =
        [
            DetectGap(input, "sa-card-header"),
            DetectGap(input, "sa-field"),
            DetectGap(input, "sa-field-set"),
            DetectGap(input, "sa-field-group"),
        ];

        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] <= values[i - 1])
            {
                values[i] = StepLadder(values[i - 1], 1);
            }
        }

        string[] names = ["xs", "sm", "md", "lg", "xl"];
        double[] ladder = [.. values, StepLadder(values[^1], 3)];

        for (var i = 0; i < names.Length; i++)
        {
            scaleVariables[$"sa-token-gap-y-{names[i]}"] = $"gap-y-{Format(ladder[i])}";
            scaleVariables[$"sa-token-gap-x-{names[i]}"] = $"gap-x-{Format(ladder[i])}";
        }

        /*// The page gutter is a different axis to the gap ladder, so it comes from --card-spacing —
        // each theme's chrome-padding number — and ramps two ladder steps either side of it.
        var padding = DetectCardSpacing(input);
        scaleVariables["sa-container-padding"] =
            $"px-{Format(StepLadder(padding, -2))} "
            + $"sm:px-{Format(padding)} "
            + $"lg:px-{Format(StepLadder(padding, 2))}";*/

        return scaleVariables;
    }

    /// <summary>Moves <paramref name="value" /> <paramref name="steps" /> places along the spacing ladder, clamped at both ends.</summary>
    private static double StepLadder(double value, int steps)
    {
        var index = Array.IndexOf(SpacingLadder, value);

        if (index < 0)
        {
            // Not a ladder value (a theme could use anything) — start from the nearest one below.
            index = Array.FindLastIndex(SpacingLadder, candidate => candidate <= value);

            if (index < 0)
            {
                index = 0;
            }
        }

        return SpacingLadder[Math.Clamp(index + steps, 0, SpacingLadder.Length - 1)];
    }
}
