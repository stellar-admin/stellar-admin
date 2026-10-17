namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of its trigger that a tooltip appears on.
/// </summary>
public enum TooltipSide
{
    /// <summary>
    ///     Above the trigger.
    /// </summary>
    Top,

    /// <summary>
    ///     To the right of the trigger.
    /// </summary>
    Right,

    /// <summary>
    ///     Below the trigger.
    /// </summary>
    Bottom,

    /// <summary>
    ///     To the left of the trigger.
    /// </summary>
    Left,
}

internal static class TooltipSideExtensions
{
    extension(TooltipSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                TooltipSide.Top => "top",
                TooltipSide.Bottom => "bottom",
                TooltipSide.Left => "left",
                TooltipSide.Right => "right",
                _ => string.Empty,
            };
    }
}
