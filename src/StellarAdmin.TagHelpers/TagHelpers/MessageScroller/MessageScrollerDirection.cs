namespace StellarAdmin.TagHelpers;

/// <summary>
///     The end of the transcript a message scroller button scrolls to.
/// </summary>
public enum MessageScrollerDirection
{
    /// <summary>Scrolls to the oldest content, at the top of the transcript.</summary>
    Start,

    /// <summary>Scrolls to the newest content, at the bottom of the transcript.</summary>
    End,
}

internal static class MessageScrollerDirectionExtensions
{
    extension(MessageScrollerDirection direction)
    {
        public string GetDataAttributeText() =>
            direction switch
            {
                MessageScrollerDirection.Start => "start",
                MessageScrollerDirection.End => "end",
                _ => string.Empty,
            };
    }
}
