using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

// Requested starts per tier; an unset tier inherits from the next smaller tier, and each resolved
// start is dropped where the parent grid has one column or the item would pass its last column.
internal readonly record struct FormColumnStartDefinitions(
    int? Base = null,
    int? Small = null,
    int? Medium = null,
    int? Large = null
)
{
    public GridTiers Resolve(GridTiers columns, GridTiers span) =>
        ResponsiveGrid.ResolveStart(columns, span, Base, Small, Medium, Large);
}
