namespace StellarAdmin.TagHelpers;

/// <summary>
///     The horizontal edge of the bubble that a reactions row is anchored to.
/// </summary>
public enum BubbleReactionsAlign
{
    /// <summary>Anchors the reactions row near the leading edge of the bubble.</summary>
    Start,

    /// <summary>Anchors the reactions row near the trailing edge of the bubble.</summary>
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
