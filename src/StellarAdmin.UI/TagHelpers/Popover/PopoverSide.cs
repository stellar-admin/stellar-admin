namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of its trigger that a popover appears on.
/// </summary>
public enum PopoverSide
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

internal static class PopoverSideExtensions
{
    extension(PopoverSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                PopoverSide.Top => "top",
                PopoverSide.Bottom => "bottom",
                PopoverSide.Left => "left",
                PopoverSide.Right => "right",
                _ => string.Empty,
            };
    }
}
