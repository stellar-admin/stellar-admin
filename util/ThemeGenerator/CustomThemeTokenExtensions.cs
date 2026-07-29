using System.Globalization;
using System.Text.RegularExpressions;

namespace ThemeGenerator;

public static partial class CustomThemeTokenExtensions
{
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
        ///     Derives a five-step spacing ladder for StellarAdmin's own components (sa-stack,
        ///     sa-group, sa-app-header), which have no upstream .cn-* block to extract, and returns
        ///     it as CSS custom properties (<c>--sa-gap-xs</c> … <c>--sa-gap-xl</c>) for the theme
        ///     file's <c>:root</c> block. The structural rules in components.css consume them via
        ///     <c>var(--sa-gap-*)</c>, so adding a theme-aware component is a components.css edit
        ///     rather than a generator change.
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
        public Dictionary<string, string> ExtractSpacingVariables()
        {
            var variables = new Dictionary<string, string>();

            double[] values =
            [
                DetectGap(tokens, "sa-card-header"),
                DetectGap(tokens, "sa-field"),
                DetectGap(tokens, "sa-field-set"),
                DetectGap(tokens, "sa-field-group"),
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
                variables[$"--sa-gap-{names[i]}"] = $"--spacing({Format(ladder[i])})";
            }

            /*// The page gutter is a different axis to the gap ladder, so it comes from --card-spacing —
            // each theme's chrome-padding number — and ramps two ladder steps either side of it.
            var padding = DetectCardSpacing(tokens);
            variables["sa-container-padding"] =
                $"px-{Format(StepLadder(padding, -2))} "
                + $"sm:px-{Format(padding)} "
                + $"lg:px-{Format(StepLadder(padding, 2))}";*/

            return variables;
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
