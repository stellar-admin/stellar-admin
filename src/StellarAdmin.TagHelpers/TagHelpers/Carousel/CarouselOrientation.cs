namespace StellarAdmin.TagHelpers;

/// <summary>The direction in which carousel items scroll.</summary>
public enum CarouselOrientation
{
    /// <summary>Items scroll horizontally.</summary>
    Horizontal,

    /// <summary>Items scroll vertically.</summary>
    Vertical,
}

internal static class CarouselOrientationExtensions
{
    extension(CarouselOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                CarouselOrientation.Horizontal => "horizontal",
                CarouselOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
    }
}
