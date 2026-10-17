namespace StellarAdmin.UI;

/// <summary>
///     Specifies the direction in which the slides of a carousel scroll.
/// </summary>
public enum CarouselOrientation
{
    /// <summary>
    ///     The slides scroll horizontally.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The slides scroll vertically.
    /// </summary>
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
