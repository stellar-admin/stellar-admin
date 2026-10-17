namespace StellarAdmin.Sidebar;

/// <summary>
///     Represents a logo that shows an icon in an accent tile beside the title.
/// </summary>
public sealed class IconBrandingLogo : BrandingLogo
{
    /// <summary>
    ///     Gets the name of the icon.
    /// </summary>
    public string Name { get; }

    internal IconBrandingLogo(string name) => Name = name;
}
