namespace StellarAdmin.UI;

/// <summary>
///     Specifies what the marker of a timeline item with no media shows.
/// </summary>
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

internal static class TimelineMarkerExtensions
{
    extension(TimelineMarker marker)
    {
        public string GetDataAttributeText()
        {
            return marker switch
            {
                TimelineMarker.Dot => "dot",
                TimelineMarker.Ring => "ring",
                TimelineMarker.Number => "number",
                _ => string.Empty,
            };
        }
    }
}
