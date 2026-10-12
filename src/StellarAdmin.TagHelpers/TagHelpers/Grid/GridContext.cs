namespace StellarAdmin.TagHelpers;

internal sealed class GridContext
{
    public required GridTiers Columns { get; init; }

    public required StellarAdminTagHelperBase Owner { get; init; }
}
