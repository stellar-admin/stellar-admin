using System.Globalization;

namespace StellarAdmin.UI;

/// <summary>
///     The shared state a <c>sa-slider</c> publishes for its marks: the resolved bounds, values,
///     layout and value format, so every mark positions, styles and labels itself like the thumbs.
/// </summary>
internal sealed class SliderContext
{
    public SliderClassNames? ClassNames { get; init; }

    public required double Min { get; init; }

    public required double Max { get; init; }

    public required double Step { get; init; }

    // The decimal places of the minimum and step, which every stepped value shares
    public required int Decimals { get; init; }

    public required IReadOnlyList<double> Values { get; init; }

    public required SliderOrientation Orientation { get; init; }

    public required SliderThumbAlignment ThumbAlignment { get; init; }

    /// <summary>
    ///     The template each displayed number is substituted into at its <c>{0}</c> placeholder, or
    ///     <c>null</c> to display the number alone.
    /// </summary>
    public string? ValueFormat { get; init; }

    public required CultureInfo Culture { get; init; }

    public string FormatValue(double value) => FormatValue(value, ValueFormat, Culture, Decimals);

    /// <summary>
    ///     Whether <paramref name="value" /> lies within the filled range: between the lowest and
    ///     highest thumb of a range slider, or from the start up to the single thumb.
    /// </summary>
    public bool IsInRange(double value) =>
        Values.Count > 1 ? value >= Values.Min() && value <= Values.Max() : value <= Values[0];

    // Matches the client, which formats with Intl.NumberFormat and up to the slider's decimals
    public static string FormatValue(
        double value,
        string? format,
        CultureInfo culture,
        int decimals
    )
    {
        var number = value.ToString("#,0." + new string('#', decimals), culture);

        return format?.Replace("{0}", number, StringComparison.Ordinal) ?? number;
    }

    public static int CountDecimals(double value)
    {
        // Dividing by a one with trailing zeros normalises the decimal's scale
        var normalized = (decimal)value / 1.000000000000000000000000000000000m;

        return normalized.Scale;
    }

    // Rounds away the floating-point noise that stepping by a fraction accumulates
    public static double Round(double value, int decimals) => Math.Round(value, decimals);
}
