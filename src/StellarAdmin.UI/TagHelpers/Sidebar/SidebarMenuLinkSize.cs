namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar menu link.
/// </summary>
public enum SidebarMenuLinkSize
{
    /// <summary>
    ///     The standard height.
    /// </summary>
    Default,

    /// <summary>
    ///     A shorter, more compact link.
    /// </summary>
    Small,

    /// <summary>
    ///     A taller link for prominent entries.
    /// </summary>
    Large,
}

internal static class SidebarMenuLinkSizeExtensions
{
    extension(SidebarMenuLinkSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                SidebarMenuLinkSize.Default => "default",
                SidebarMenuLinkSize.Small => "sm",
                SidebarMenuLinkSize.Large => "lg",
                _ => String.Empty,
            };
        }
    }
}
