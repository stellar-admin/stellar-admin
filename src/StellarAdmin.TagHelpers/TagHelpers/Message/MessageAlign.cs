namespace StellarAdmin.TagHelpers;

/// <summary>
///     The side of the conversation a message sits on.
/// </summary>
public enum MessageAlign
{
    /// <summary>Aligns the message to the start of the conversation, for incoming messages.</summary>
    Start,

    /// <summary>Aligns the message to the end of the conversation, for outgoing messages.</summary>
    End,
}

internal static class MessageAlignExtensions
{
    extension(MessageAlign align)
    {
        public string GetDataAttributeText() =>
            align switch
            {
                MessageAlign.Start => "start",
                MessageAlign.End => "end",
                _ => string.Empty,
            };
    }
}
