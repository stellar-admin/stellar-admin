namespace StellarAdmin.UI;

/// <summary>
///     The size of a sidebar submenu link.
/// </summary>
public enum SidebarMenuSubLinkSize
{
    /// <summary>The standard size.</summary>
    Default,

    /// <summary>A more compact size.</summary>
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
