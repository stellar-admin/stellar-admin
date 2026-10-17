namespace StellarAdmin.UI;

/// <summary>
///     Specifies the edge of the screen that a sidebar sits on.
/// </summary>
public enum SidebarSide
{
    /// <summary>
    ///     The sidebar sits on the left edge.
    /// </summary>
    Left,

    /// <summary>
    ///     The sidebar sits on the right edge.
    /// </summary>
    Right,
}

internal static class SidebarSideExtensions
{
    extension(SidebarSide side)
    {
        public string GetDataAttributeText() =>
            side switch
            {
                SidebarSide.Left => "left",
                SidebarSide.Right => "right",
                _ => string.Empty,
            };
    }
}
