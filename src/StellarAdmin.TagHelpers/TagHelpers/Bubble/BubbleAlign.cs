namespace StellarAdmin.TagHelpers;

/// <summary>
///     The side of the conversation a bubble sits on.
/// </summary>
public enum BubbleAlign
{
    /// <summary>Aligns the bubble to the start of the conversation, for incoming messages.</summary>
    Start,

    /// <summary>Aligns the bubble to the end of the conversation, for outgoing messages.</summary>
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
