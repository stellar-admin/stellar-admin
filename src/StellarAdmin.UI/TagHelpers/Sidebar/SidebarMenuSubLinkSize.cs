namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar submenu link.
/// </summary>
[GenerateDataAttributeText]
public enum SidebarMenuSubLinkSize
{
    /// <summary>
    ///     The standard text size.
    /// </summary>
    [DataAttributeText("md")]
    Default,

    /// <summary>
    ///     A smaller text size.
    /// </summary>
    [DataAttributeText("sm")]
    Small,
}
