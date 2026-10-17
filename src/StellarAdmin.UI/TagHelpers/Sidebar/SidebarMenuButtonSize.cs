namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar menu button.
/// </summary>
public enum SidebarMenuButtonSize
{
    /// <summary>
    ///     The standard height.
    /// </summary>
    Default,

    /// <summary>
    ///     A shorter, more compact button.
    /// </summary>
    Small,

    /// <summary>
    ///     A taller button for prominent entries.
    /// </summary>
    Large,
}

internal static class SidebarMenuButtonSizeExtensions
{
    extension(SidebarMenuButtonSize size)
    {
        public string GetDataAttributeText()
        {
            return size switch
            {
                SidebarMenuButtonSize.Default => "default",
                SidebarMenuButtonSize.Small => "sm",
                SidebarMenuButtonSize.Large => "lg",
                _ => String.Empty,
            };
        }
    }
}
