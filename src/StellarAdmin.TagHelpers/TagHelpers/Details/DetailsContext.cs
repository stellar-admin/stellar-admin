namespace StellarAdmin.TagHelpers;

internal sealed class DetailsContext
{
    public required GridTiers Columns { get; init; }

    public required FieldOrientation? Orientation { get; init; }
}
