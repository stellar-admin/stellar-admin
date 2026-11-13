namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a slider thumb is positioned relative to its value.
/// </summary>
[GenerateDataAttributeText]
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
