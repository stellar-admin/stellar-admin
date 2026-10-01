using System.Globalization;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The shared state a <c>sa-slider</c> publishes for its marks: the resolved bounds, values,
///     layout and value format, so every mark positions, styles and labels itself like the thumbs.
/// </summary>
internal sealed class SliderContext
{
    public SliderClassNames? ClassNames { get; init; }

    public required int Min { get; init; }

    public required int Max { get; init; }

    public required int Step { get; init; }

    public required IReadOnlyList<int> Values { get; init; }

    public required SliderOrientation Orientation { get; init; }

    public required SliderThumbAlignment ThumbAlignment { get; init; }

    /// <summary>
    ///     The template each displayed number is substituted into at its <c>{0}</c> placeholder, or
    ///     <c>null</c> to display the number alone.
    /// </summary>
    public string? ValueFormat { get; init; }

    public required CultureInfo Culture { get; init; }

    public string FormatValue(int value) => FormatValue(value, ValueFormat, Culture);

    /// <summary>
    ///     Whether <paramref name="value" /> lies within the filled range: between the lowest and
    ///     highest thumb of a range slider, or from the start up to the single thumb.
    /// </summary>
    public bool IsInRange(int value) =>
        Values.Count > 1 ? value >= Values.Min() && value <= Values.Max() : value <= Values[0];

    // Matches the client, which formats with Intl.NumberFormat and no fraction digits
    public static string FormatValue(int value, string? format, CultureInfo culture)
    {
        var number = value.ToString("N0", culture);

        return format?.Replace("{0}", number, StringComparison.Ordinal) ?? number;
    }
}
