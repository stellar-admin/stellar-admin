namespace StellarAdmin.UI;

/// <summary>
///     Specifies the line drawn from a timeline item to the next one.
/// </summary>
[GenerateDataAttributeText]
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
