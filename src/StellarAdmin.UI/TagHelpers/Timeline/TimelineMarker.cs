namespace StellarAdmin.UI;

/// <summary>
///     Specifies what the marker of a timeline item with no media shows.
/// </summary>
[GenerateDataAttributeText]
public enum TimelineMarker
{
    /// <summary>
    ///     A filled dot.
    /// </summary>
    Dot,

    /// <summary>
    ///     A hollow ring.
    /// </summary>
    Ring,

    /// <summary>
    ///     The item's number in the timeline, in a ring.
    /// </summary>
    Number,
}
