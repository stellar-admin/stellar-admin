namespace StellarAdmin.UI;

/// <summary>
///     The size of a sidebar submenu button.
/// </summary>
public enum SidebarMenuSubButtonSize
{
    /// <summary>The standard size.</summary>
    Default,

    /// <summary>A more compact size.</summary>
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
