using Microsoft.AspNetCore.Authorization;
using StellarAdmin.Infrastructure.Authorization;

namespace StellarAdmin.Sidebar;

/// <summary>
///     Configures a sidebar link.
/// </summary>
public sealed class SidebarLinkBuilder
{
    private readonly SidebarLinkOptions _options;

    /// <summary>
    ///     Sets the label of the sidebar group that the link is in.
    /// </summary>
    /// <value>
    ///     The group label, or <see langword="null" /> to place the link outside any group. The
    ///     default is <see langword="null" />.
    /// </value>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    /// <remarks>
    ///     Links and resources with the same group label share a group.
    /// </remarks>
    public string? Group
    {
        set => _options.Group = value;
    }

    /// <summary>
    ///     Sets the icon shown before the link's label.
    /// </summary>
    /// <value>
    ///     The name of the icon, or <see langword="null" /> for none. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <exception cref="ArgumentException">
    ///     The value is empty or white space.
    /// </exception>
    public string? Icon
    {
        set => _options.Icon = value;
    }

    /// <summary>
    ///     Sets a value that indicates whether the link opens in a new browser tab.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the link opens in a new tab; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    public bool OpenInNewTab
    {
        set => _options.OpenInNewTab = value;
    }

    /// <summary>
    ///     Sets the position of the link among the links around it.
    /// </summary>
    /// <value>
    ///     The position, where lower values come first. The default is 0.
    /// </value>
    /// <remarks>
    ///     Links in a group are sorted by order. Links outside any group are sorted with the
    ///     ungrouped links next to them.
    /// </remarks>
    public int Order
    {
        set => _options.Order = value;
    }

    internal SidebarLinkBuilder(SidebarLinkOptions options) => _options = options;

    /// <summary>
    ///     Shows the link only to authenticated users.
    /// </summary>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Hiding the link doesn't protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Shows the link only to users who satisfy an authorization policy.
    /// </summary>
    /// <param name="policy">The authorization policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policy" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Hiding the link doesn't protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization(AuthorizationPolicy policy)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policy));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy an authorization policy that a delegate builds.
    /// </summary>
    /// <param name="configurePolicy">A delegate that builds the authorization policy.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configurePolicy" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Hiding the link doesn't protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization(
        Action<AuthorizationPolicyBuilder> configurePolicy
    )
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(configurePolicy));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy authorization data.
    /// </summary>
    /// <param name="authorizeData">
    ///     The authorization data. When it's empty, the user must be authenticated.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="authorizeData" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Hiding the link doesn't protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(authorizeData));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy the named authorization policies.
    /// </summary>
    /// <param name="policyNames">
    ///     The names of the authorization policies. When it's empty, the user must be
    ///     authenticated.
    /// </param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="policyNames" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Hiding the link doesn't protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization(params string[] policyNames)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policyNames));

        return this;
    }
}
