namespace StellarAdmin.UI;

/// <summary>
///     Specifies the size of a sidebar menu button.
/// </summary>
[GenerateDataAttributeText]
public enum SidebarMenuButtonSize
{
    /// <summary>
    ///     The standard height.
    /// </summary>
    Default,

    /// <summary>
    ///     A shorter, more compact button.
    /// </summary>
    [DataAttributeText("sm")]
    Small,

    /// <summary>
    ///     A taller button for prominent entries.
    /// </summary>
    [DataAttributeText("lg")]
    Large,
}
