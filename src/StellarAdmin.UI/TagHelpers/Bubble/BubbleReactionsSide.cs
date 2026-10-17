namespace StellarAdmin.UI;

/// <summary>
///     Specifies the vertical edge of a bubble that its reactions sit on.
/// </summary>
public enum BubbleReactionsSide
{
    /// <summary>
    ///     On the top edge of the bubble.
    /// </summary>
    Top,

    /// <summary>
    ///     On the bottom edge of the bubble.
    /// </summary>
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
