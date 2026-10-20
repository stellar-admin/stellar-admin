namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a timeline, which sets the size of its markers and the spacing between its items.
/// </summary>
public enum TimelineSize
{
    /// <summary>
    ///     The standard size.
    /// </summary>
    Default,

    /// <summary>
    ///     A compact size, for a timeline in a card or a sidebar.
    /// </summary>
    Small,
}

internal static class TimelineSizeExtensions
{
    extension(TimelineSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                TimelineSize.Default => "default",
                TimelineSize.Small => "sm",
                _ => string.Empty,
            };
        }
    }
}
