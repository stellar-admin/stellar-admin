namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of the conversation that a message sits on.
/// </summary>
[GenerateDataAttributeText]
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
