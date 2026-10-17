namespace StellarAdmin.UI;

/// <summary>
///     Specifies the horizontal edge of a bubble that its reactions sit at.
/// </summary>
public enum BubbleReactionsAlign
{
    /// <summary>
    ///     Near the start edge of the bubble.
    /// </summary>
    Start,

    /// <summary>
    ///     Near the end edge of the bubble.
    /// </summary>
    End,
}

internal static class BubbleReactionsAlignExtensions
{
    extension(BubbleReactionsAlign align)
    {
        public string GetDataAttributeText() =>
            align switch
            {
                BubbleReactionsAlign.Start => "start",
                BubbleReactionsAlign.End => "end",
                _ => string.Empty,
            };
    }
}
