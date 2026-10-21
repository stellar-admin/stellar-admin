namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures the sidebar's user menu, which shows the signed-in user and their menu items.
/// </summary>
public sealed class UserMenuBuilder
{
    private readonly UserMenuOptions _options;

    /// <summary>
    ///     Sets the type of the claim whose value is the URL of the user's picture.
    /// </summary>
    /// <value>
    ///     The claim type. The default is <c>picture</c>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Without a picture, the avatar shows the user's initials.
    /// </remarks>
    public string AvatarClaimType
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _options.AvatarClaimType = value;
        }
    }

    /// <summary>
    ///     Sets the type of the claim shown under the user's name.
    /// </summary>
    /// <value>
    ///     The claim type. The default is the email address, from the
    ///     <see cref="System.Security.Claims.ClaimTypes.Email" /> or <c>email</c> claim.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     A claim with several values, such as one role claim per role, shows them all. Without
    ///     the claim, the menu shows the name alone.
    /// </remarks>
    public string DescriptionClaimType
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _options.DescriptionClaimType = value;
        }
    }

    /// <summary>
    ///     Sets the type of the claim shown as the user's name.
    /// </summary>
    /// <value>
    ///     The claim type. The default is the identity's name, then the
    ///     <see cref="System.Security.Claims.ClaimTypes.Name" /> or <c>name</c> claim.
    /// </value>
    /// <exception cref="ArgumentNullException">
    ///     The value is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Without the claim, the menu shows the description in place of the name.
    /// </remarks>
    public string NameClaimType
    {
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _options.NameClaimType = value;
        }
    }

    internal UserMenuBuilder(UserMenuOptions options) => _options = options;

    /// <summary>
    ///     Adds an item to the end of the menu.
    /// </summary>
    /// <param name="item">
    ///     The item to add, created with one of the static methods of <see cref="UserMenuItem" />.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="item" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The log out item is always shown after the other items.
    /// </remarks>
    public UserMenuBuilder Add(UserMenuItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        _options.Items.Add(item);

        return this;
    }

    /// <summary>
    ///     Removes the items added so far.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     The log out item isn't removed. Hide it with <see cref="Logout" />.
    /// </remarks>
    public UserMenuBuilder Clear()
    {
        _options.Items.Clear();

        return this;
    }

    /// <summary>
    ///     Configures the log out item and the logout endpoint it posts to.
    /// </summary>
    /// <param name="configure">A delegate that configures log out.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    public UserMenuBuilder Logout(Action<LogoutBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new(_options.Logout));

        return this;
    }
}
