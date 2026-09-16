using Microsoft.AspNetCore.Identity;
using StellarAdmin.Dashboard.Identity.Options;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.Identity.Builders;

/// <summary>
///     Configures the StellarAdmin Identity user and role management screens for the
///     Identity user type <typeparamref name="TUser" /> and role type
///     <typeparamref name="TRole" />.
/// </summary>
/// <typeparam name="TUser">
///     The Identity user type the host registered with <c>AddIdentity</c>.
/// </typeparam>
/// <typeparam name="TRole">
///     The Identity role type the host registered with <c>AddIdentity</c> or <c>AddRoles</c>.
/// </typeparam>
/// <typeparam name="TKey">
///     The type of the primary key for a user and a role.
/// </typeparam>
public class StellarAdminIdentityBuilder<TUser, TRole, TKey>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly StellarAdminIdentityOptions _options;
    private readonly IdentityRolesOptions<TRole, TKey> _rolesOptions;
    private readonly IdentityUsersOptions<TUser, TKey> _usersOptions;

    internal StellarAdminIdentityBuilder(
        StellarAdminIdentityOptions options,
        IdentityUsersOptions<TUser, TKey> usersOptions,
        IdentityRolesOptions<TRole, TKey> rolesOptions
    )
    {
        _options = options;
        _usersOptions = usersOptions;
        _rolesOptions = rolesOptions;
    }

    /// <summary>
    ///     Configures the role management screens.
    /// </summary>
    public StellarAdminIdentityBuilder<TUser, TRole, TKey> ConfigureRoles(
        Action<ResourceBuilder<TRole>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new ResourceBuilder<TRole>(_rolesOptions));

        return this;
    }

    /// <summary>
    ///     Configures the Identity section of the StellarAdmin sidebar.
    /// </summary>
    public StellarAdminIdentityBuilder<TUser, TRole, TKey> ConfigureSidebar(
        Action<IdentitySidebarBuilder> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new IdentitySidebarBuilder(_options.Sidebar));

        return this;
    }

    /// <summary>
    ///     Configures the user management screens.
    /// </summary>
    public StellarAdminIdentityBuilder<TUser, TRole, TKey> ConfigureUsers(
        Action<ResourceBuilder<TUser>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new ResourceBuilder<TUser>(_usersOptions));

        return this;
    }
}
