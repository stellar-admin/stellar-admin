namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a tooltip lines up with its trigger along the side it appears on.
/// </summary>
/// <remarks>
///     Above or below the trigger, the start is the trigger's left edge. To its left or right,
///     the start is the trigger's top edge.
/// </remarks>
public enum TooltipAlign
{
    /// <summary>
    ///     Lines up with the trigger's start edge and extends past its end edge.
    /// </summary>
    Start,

    /// <summary>
    ///     Centers on the trigger and extends past both of its edges as far as the content needs.
    /// </summary>
    Center,

    /// <summary>
    ///     Lines up with the trigger's end edge and extends past its start edge.
    /// </summary>
    End,

    /// <summary>
    ///     Centers on the trigger and stays within its width or height, so text wraps to the
    ///     trigger's width.
    /// </summary>
    CenterWithin,
}

internal static class TooltipAlignExtensions
{
    extension(TooltipAlign align)
    {
        public string GetDataAttributeText() =>
            align switch
            {
                TooltipAlign.Start => "start",
                TooltipAlign.Center => "center",
                TooltipAlign.End => "end",
                TooltipAlign.CenterWithin => "center-within",
                _ => string.Empty,
            };
    }
}
