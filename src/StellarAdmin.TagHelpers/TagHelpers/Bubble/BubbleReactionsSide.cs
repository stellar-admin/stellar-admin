namespace StellarAdmin.TagHelpers;

/// <summary>
///     The vertical edge of the bubble that a reactions row is anchored to.
/// </summary>
public enum BubbleReactionsSide
{
    /// <summary>Anchors the reactions row to the top edge of the bubble.</summary>
    Top,

    /// <summary>Anchors the reactions row to the bottom edge of the bubble.</summary>
    Bottom,
}

internal static class BubbleReactionsSideExtensions
{
    extension(BubbleReactionsSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                BubbleReactionsSide.Top => "top",
                BubbleReactionsSide.Bottom => "bottom",
                _ => string.Empty,
            };
    }
}
