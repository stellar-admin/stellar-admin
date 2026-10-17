namespace StellarAdmin.UI;

/// <summary>
///     Specifies the direction of a slider's track.
/// </summary>
public enum SliderOrientation
{
    /// <summary>
    ///     The track runs from left to right.
    /// </summary>
    Horizontal,

    /// <summary>
    ///     The track runs from bottom to top.
    /// </summary>
    Vertical,
}

internal static class SliderOrientationExtensions
{
    extension(SliderOrientation orientation)
    {
        public string GetDataAttributeText() =>
            orientation switch
            {
                SliderOrientation.Horizontal => "horizontal",
                SliderOrientation.Vertical => "vertical",
                _ => string.Empty,
            };
    }
}
