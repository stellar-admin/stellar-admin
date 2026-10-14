namespace StellarAdmin.Sidebar;

/// <summary>
///     A logo that shows an icon in an accent tile beside the title.
/// </summary>
public sealed class IconBrandingLogo : BrandingLogo
{
    /// <summary>
    ///     The icon name.
    /// </summary>
    public string Name { get; }

    internal IconBrandingLogo(string name) => Name = name;
}
