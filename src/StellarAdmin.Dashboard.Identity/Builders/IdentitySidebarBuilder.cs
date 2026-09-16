using StellarAdmin.Dashboard.Identity.Options;

namespace StellarAdmin.Dashboard.Identity.Builders;

/// <summary>
///     Configures the Identity section of the StellarAdmin sidebar.
/// </summary>
public class IdentitySidebarBuilder
{
    private readonly IdentitySidebarOptions _options;

    /// <summary>
    ///     Sets the sidebar section title. When <c>null</c> (the default), the title is
    ///     "Identity".
    /// </summary>
    public string? Title
    {
        get => _options.Title;
        set => _options.Title = value;
    }

    /// <summary>
    ///     Sets whether the sidebar section renders. Cosmetic only — hiding the section
    ///     does not disable the user management screens.
    /// </summary>
    public bool Visible
    {
        get => _options.Visible;
        set => _options.Visible = value;
    }

    internal IdentitySidebarBuilder(IdentitySidebarOptions options)
    {
        _options = options;
    }

    /// <summary>
    ///     Configures the roles sidebar item.
    /// </summary>
    public IdentitySidebarBuilder RolesItem(Action<SidebarItemBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new SidebarItemBuilder(_options.RolesItem));

        return this;
    }

    /// <summary>
    ///     Configures the users sidebar item.
    /// </summary>
    public IdentitySidebarBuilder UsersItem(Action<SidebarItemBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new SidebarItemBuilder(_options.UsersItem));

        return this;
    }
}
