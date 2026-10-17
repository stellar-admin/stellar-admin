namespace StellarAdmin.UI;

/// <summary>
///     Specifies the end of the transcript that a message scroller button scrolls to.
/// </summary>
public enum MessageScrollerDirection
{
    /// <summary>
    ///     The oldest content, at the top of the transcript.
    /// </summary>
    Start,

    /// <summary>
    ///     The newest content, at the bottom of the transcript.
    /// </summary>
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
