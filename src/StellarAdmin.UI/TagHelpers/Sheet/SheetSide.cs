namespace StellarAdmin.UI;

/// <summary>
///     Specifies the edge of the screen that a sheet slides in from.
/// </summary>
public enum SheetSide
{
    /// <summary>
    ///     The sheet slides in from the top edge.
    /// </summary>
    Top,

    /// <summary>
    ///     The sheet slides in from the right edge.
    /// </summary>
    Right,

    /// <summary>
    ///     The sheet slides in from the bottom edge.
    /// </summary>
    Bottom,

    /// <summary>
    ///     The sheet slides in from the left edge.
    /// </summary>
    Left,
}

internal static class SheetSideExtensions
{
    extension(SheetSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                SheetSide.Top => "top",
                SheetSide.Right => "right",
                SheetSide.Bottom => "bottom",
                SheetSide.Left => "left",
                _ => string.Empty,
            };
    }
}
