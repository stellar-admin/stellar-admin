namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of the conversation that a bubble sits on.
/// </summary>
public enum BubbleAlign
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

internal static class BubbleAlignExtensions
{
    extension(BubbleAlign align)
    {
        public string GetDataAttributeText() =>
            align switch
            {
                BubbleAlign.Start => "start",
                BubbleAlign.End => "end",
                _ => string.Empty,
            };
    }
}
