namespace StellarAdmin.UI;

/// <summary>
///     Specifies the line drawn from a timeline item to the next one.
/// </summary>
public enum TimelineRail
{
    /// <summary>
    ///     A solid line.
    /// </summary>
    Solid,

    /// <summary>
    ///     A dashed line, such as for an optional item.
    /// </summary>
    Dashed,

    /// <summary>
    ///     No line, such as for a gap the timeline doesn't cover.
    /// </summary>
    None,
}

internal static class TimelineRailExtensions
{
    extension(TimelineRail rail)
    {
        public string GetDataAttributeText()
        {
            return rail switch
            {
                TimelineRail.Solid => "solid",
                TimelineRail.Dashed => "dashed",
                TimelineRail.None => "none",
                _ => string.Empty,
            };
        }
    }
}
