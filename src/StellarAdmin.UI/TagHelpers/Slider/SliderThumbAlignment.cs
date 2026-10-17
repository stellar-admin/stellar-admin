namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a slider thumb is positioned relative to its value.
/// </summary>
public enum SliderThumbAlignment
{
    /// <summary>
    ///     The thumb is centered on its value, so it overhangs the ends of the track.
    /// </summary>
    Center,

    /// <summary>
    ///     The thumb stays within the track at the minimum and maximum.
    /// </summary>
    Edge,
}

internal static class SliderThumbAlignmentExtensions
{
    extension(SliderThumbAlignment alignment)
    {
        public string GetDataAttributeText() =>
            alignment switch
            {
                SliderThumbAlignment.Center => "center",
                SliderThumbAlignment.Edge => "edge",
                _ => string.Empty,
            };
    }
}
