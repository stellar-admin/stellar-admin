namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a timeline, which sets the size of its markers and the spacing between its items.
/// </summary>
[GenerateDataAttributeText]
public enum TimelineSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size, for a timeline in a card or a sidebar.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
