namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar submenu link.
/// </summary>
public enum SidebarMenuSubLinkSize
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

internal static class SidebarMenuSubLinkSizeExtensions
{
    extension(SidebarMenuSubLinkSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                SidebarMenuSubLinkSize.Default => "md",
                SidebarMenuSubLinkSize.Small => "sm",
                _ => string.Empty,
            };
        }
    }
}
