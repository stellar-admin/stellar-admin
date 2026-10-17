namespace StellarAdmin.UI;

/// <summary>
///     The visual style of a dropdown menu item.
/// </summary>
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

internal static class DropdownMenuItemVariantExtensions
{
    extension(DropdownMenuItemVariant variant)
    {
        public string GetDataAttributeText() =>
            variant switch
            {
                DropdownMenuItemVariant.Default => "default",
                DropdownMenuItemVariant.Destructive => "destructive",
                _ => string.Empty,
            };
    }
}
