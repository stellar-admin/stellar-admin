namespace StellarAdmin.TagHelpers;

/// <summary>
///     Where a message scroller is scrolled to when it first appears.
/// </summary>
public enum MessageScrollerPosition
{
    /// <summary>The oldest content, at the top of the transcript.</summary>
    Start,

    /// <summary>The newest content, at the bottom of the transcript.</summary>
    End,
}

internal static class MessageScrollerPositionExtensions
{
    extension(MessageScrollerPosition position)
    {
        public string GetDataAttributeText() =>
            position switch
            {
                MessageScrollerPosition.Start => "start",
                MessageScrollerPosition.End => "end",
                _ => string.Empty,
            };
    }
}
