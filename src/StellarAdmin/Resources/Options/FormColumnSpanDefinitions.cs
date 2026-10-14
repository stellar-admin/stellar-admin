using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

// Requested spans per tier; an unset tier inherits from the next smaller tier, and each
// resolved span is clamped to the parent grid's columns at that tier.
internal readonly record struct FormColumnSpanDefinitions(
    int? Base = null,
    int? Small = null,
    int? Medium = null,
    int? Large = null
)
{
    public GridTiers Resolve(GridTiers columns) =>
        ResponsiveGrid.ResolveSpan(columns, Base, Small, Medium, Large);
}
