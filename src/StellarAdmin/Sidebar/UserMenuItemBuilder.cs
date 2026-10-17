namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures a user menu item.
/// </summary>
public sealed class UserMenuItemBuilder
{
    private readonly Action<string?> _setIcon;

    /// <summary>
    ///     Sets the icon shown before the item's text.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Icon
    {
        set => _setIcon(value);
    }

    internal UserMenuItemBuilder(Action<string?> setIcon) => _setIcon = setIcon;
}
