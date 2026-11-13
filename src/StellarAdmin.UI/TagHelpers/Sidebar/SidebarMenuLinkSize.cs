namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar menu link.
/// </summary>
[GenerateDataAttributeText]
public enum SidebarMenuLinkSize
{
    /// <summary>
    ///     The standard height.
    /// </summary>
    Default,

    /// <summary>
    ///     A shorter, more compact link.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A taller link for prominent entries.
    /// </summary>
    [DataAttributeText("lg")]
    Large,
}
