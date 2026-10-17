namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the sidebar's user menu, which shows the signed-in user and their menu items.
/// </summary>
public sealed class UserMenuBuilder
{
    private readonly UserMenuOptions _options;

    /// <summary>
    ///     The claim whose value is the URL of the user's picture.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>picture</c>. Without a picture, the avatar shows the user's initials.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string AvatarClaimType
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.AvatarClaimType = value;
        }
    }

    /// <summary>
    ///     The claim shown under the user's name.
    /// </summary>
    /// <remarks>
    ///     Defaults to the email address claim, <see cref="System.Security.Claims.ClaimTypes.Email" /> or
    ///     <c>email</c>. Without the claim, the menu shows the name alone.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string DescriptionClaimType
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.DescriptionClaimType = value;
        }
    }

    /// <summary>
    ///     The claim shown as the user's name.
    /// </summary>
    /// <remarks>
    ///     Defaults to the identity's name, then the <see cref="System.Security.Claims.ClaimTypes.Name" />
    ///     and <c>name</c> claims.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string NameClaimType
    {
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);

            _options.NameClaimType = value;
        }
    }

    internal UserMenuBuilder(UserMenuOptions options) => _options = options;

    /// <summary>
    ///     Adds an item to the end of the menu. The log out item always shows after the items.
    /// </summary>
    /// <param name="item">The item, created with a <see cref="UserMenuItem" /> factory.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UserMenuBuilder Add(UserMenuItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _options.Items.Add(item);

        return this;
    }

    /// <summary>
    ///     Removes the items added so far. The log out item is configured with <see cref="Logout" />.
    /// </summary>
    public UserMenuBuilder Clear()
    {
        _options.Items.Clear();

        return this;
    }

    /// <summary>
    ///     Configures the log out item, which shows after the other items, and the admin panel's logout endpoint
    ///     it posts to.
    /// </summary>
    /// <param name="configure">Configures log out.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UserMenuBuilder Logout(Action<LogoutBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_options.Logout));

        return this;
    }
}
