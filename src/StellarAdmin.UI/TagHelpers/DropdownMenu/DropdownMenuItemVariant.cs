namespace StellarAdmin.UI;

/// <summary>
///     The visual style of a dropdown menu item.
/// </summary>
[GenerateDataAttributeText]
public enum DropdownMenuItemVariant
{
    /// <summary>
    ///     Uses the standard item style.
    /// </summary>
    Default,

    /// <summary>
    ///     Uses a destructive style, for actions such as delete.
    /// </summary>
    Destructive,
}
