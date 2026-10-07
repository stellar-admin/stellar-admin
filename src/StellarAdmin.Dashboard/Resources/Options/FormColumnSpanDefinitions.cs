namespace StellarAdmin.Dashboard.Resources.Options;

// Requested spans per tier; an unset tier inherits from the next smaller tier, and each
// resolved span is clamped to the parent grid's columns at that tier.
internal readonly record struct FormColumnSpanDefinitions(
    int? Default = null,
    int? Small = null,
    int? Medium = null,
    int? Large = null
)
{
    public const int Full = int.MaxValue;

    public FormGridTiers Resolve(FormGridTiers columns)
    {
        var defaultSpan = Default ?? 1;
        var smallSpan = Small ?? defaultSpan;
        var mediumSpan = Medium ?? smallSpan;
        var largeSpan = Large ?? mediumSpan;

        return new(
            Math.Min(defaultSpan, columns.Default),
            Math.Min(smallSpan, columns.Small),
            Math.Min(mediumSpan, columns.Medium),
            Math.Min(largeSpan, columns.Large)
        );
    }
}
