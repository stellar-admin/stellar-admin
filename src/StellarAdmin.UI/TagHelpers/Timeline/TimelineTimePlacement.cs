namespace StellarAdmin.UI;

/// <summary>
///     Specifies where the time of each timeline item is shown.
/// </summary>
public enum TimelineTimePlacement
{
    /// <summary>
    ///     At the end of the item's title row.
    /// </summary>
    End,

    /// <summary>
    ///     Under the item's title.
    /// </summary>
    Below,

    /// <summary>
    ///     In a column before the markers, as wide as its longest time, so the times line up.
    /// </summary>
    Start,
}

internal static class TimelineTimePlacementExtensions
{
    extension(TimelineTimePlacement timePlacement)
    {
        public string GetDataAttributeText()
        {
            return timePlacement switch
            {
                TimelineTimePlacement.End => "end",
                TimelineTimePlacement.Below => "below",
                TimelineTimePlacement.Start => "start",
                _ => string.Empty,
            };
        }
    }
}
