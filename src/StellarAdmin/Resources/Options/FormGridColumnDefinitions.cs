using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

// Requested column counts per tier; an unset tier inherits from the next smaller tier.
internal readonly record struct FormGridColumnDefinitions(
    int? Base = null,
    int? Small = null,
    int? Medium = null,
    int? Large = null
)
{
    // A plain count keeps one column below the medium tier.
    public static FormGridColumnDefinitions FromCount(int count) =>
        new(Medium: ResponsiveGrid.ValidateColumns(count, nameof(count)));

    public GridTiers Resolve() => ResponsiveGrid.ResolveColumns(Base, Small, Medium, Large);
}
