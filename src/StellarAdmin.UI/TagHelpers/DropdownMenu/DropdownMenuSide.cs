namespace StellarAdmin.UI;

/// <summary>
///     Specifies the side of its trigger that a dropdown menu panel appears on.
/// </summary>
public enum DropdownMenuSide
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

internal static class DropdownMenuSideExtensions
{
    extension(DropdownMenuSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                DropdownMenuSide.Top => "top",
                DropdownMenuSide.Bottom => "bottom",
                DropdownMenuSide.Left => "left",
                DropdownMenuSide.Right => "right",
                _ => string.Empty,
            };
    }
}
