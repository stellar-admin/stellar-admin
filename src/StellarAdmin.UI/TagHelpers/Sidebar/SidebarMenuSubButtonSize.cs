namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar submenu button.
/// </summary>
public enum SidebarMenuSubButtonSize
{
    /// <summary>
    ///     The standard text size.
    /// </summary>
    Default,

    /// <summary>
    ///     A smaller text size.
    /// </summary>
    Small,
}

internal static class SidebarMenuSubButtonSizeExtensions
{
    extension(SidebarMenuSubButtonSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                SidebarMenuSubButtonSize.Default => "md",
                SidebarMenuSubButtonSize.Small => "sm",
                _ => string.Empty,
            };
        }
    }
}
