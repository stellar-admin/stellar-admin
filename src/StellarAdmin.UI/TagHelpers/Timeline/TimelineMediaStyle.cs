namespace StellarAdmin.UI;

/// <summary>
///     Specifies how the icon in a timeline item's media is drawn.
/// </summary>
public enum TimelineMediaStyle
{
    /// <summary>
    ///     The icon on a filled disc.
    /// </summary>
    Disc,

    /// <summary>
    ///     The icon on its own.
    /// </summary>
    Plain,

    /// <summary>
    ///     The icon in a ring.
    /// </summary>
    Ring,
}

internal static class TimelineMediaStyleExtensions
{
    extension(TimelineMediaStyle mediaStyle)
    {
        public string GetDataAttributeText()
        {
            return mediaStyle switch
            {
                TimelineMediaStyle.Disc => "disc",
                TimelineMediaStyle.Plain => "plain",
                TimelineMediaStyle.Ring => "ring",
                _ => string.Empty,
            };
        }
    }
}
