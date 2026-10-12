using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

// Requested column counts per tier; an unset tier inherits from the next smaller tier.
internal readonly record struct FormGridColumnDefinitions(
    int? Default = null,
    int? Small = null,
    int? Medium = null,
    int? Large = null
)
{
    // A plain count keeps one column below the medium tier.
    public static FormGridColumnDefinitions FromCount(int count) =>
        new(Medium: FormGrid.ValidateColumns(count, nameof(count)));

    public GridTiers Resolve() => ResponsiveGrid.ResolveColumns(Default, Small, Medium, Large);
}
