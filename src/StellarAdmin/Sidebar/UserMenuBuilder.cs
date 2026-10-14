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
    ///     The menu's items, in the order they are shown.
    /// </summary>
    /// <remarks>
    ///     Starts with a <see cref="UserMenuItem.Logout()" /> item. Insert items before it, or assign a list
    ///     to replace the items; a list without a logout item shows no log out.
    /// </remarks>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public IList<UserMenuItem> Items
    {
        get => _options.Items;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var items = value.ToList();
            if (items.Contains(null!))
            {
                throw new ArgumentException("The items cannot contain null.", nameof(value));
            }

            _options.Items.Clear();
            _options.Items.AddRange(items);
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
    ///     Configures the admin panel's logout endpoint, which <see cref="UserMenuItem.Logout()" /> posts to.
    /// </summary>
    /// <param name="configure">Configures the endpoint.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public UserMenuBuilder Logout(Action<LogoutBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_options.Logout));

        return this;
    }
}
