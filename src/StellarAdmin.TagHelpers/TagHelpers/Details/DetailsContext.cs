namespace StellarAdmin.TagHelpers;

internal sealed class DetailsContext
{
    public required int Columns { get; init; }

    public required FieldOrientation? Orientation { get; init; }
}
