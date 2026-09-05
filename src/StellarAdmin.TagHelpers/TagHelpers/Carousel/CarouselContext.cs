namespace StellarAdmin.TagHelpers;

internal sealed class CarouselContext
{
    public required string CarouselId { get; init; }

    public required CarouselOrientation Orientation { get; init; }
}
