namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures a user menu item.
/// </summary>
public sealed class UserMenuItemBuilder
{
    private readonly Action<string?> _setIcon;

    /// <summary>
    ///     The icon shown before the item's text, or <c>null</c> for none.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>null</c>, or <c>log-out</c> for a log out item.
    /// </remarks>
    public string? Icon
    {
        set => _setIcon(value);
    }

    internal UserMenuItemBuilder(Action<string?> setIcon) => _setIcon = setIcon;
}
