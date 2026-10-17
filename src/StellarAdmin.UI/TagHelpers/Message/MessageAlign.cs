namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of the conversation that a message sits on.
/// </summary>
public enum MessageAlign
{
    /// <summary>
    ///     The start of the conversation, for incoming messages.
    /// </summary>
    Start,

    /// <summary>
    ///     The end of the conversation, for outgoing messages.
    /// </summary>
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
