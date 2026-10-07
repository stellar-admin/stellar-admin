using Microsoft.AspNetCore.Authorization;
using StellarAdmin.Dashboard.Infrastructure.Authorization;

namespace StellarAdmin.Dashboard.Sidebar;

/// <summary>
///     Configures a sidebar link.
/// </summary>
public sealed class SidebarLinkBuilder
{
    private readonly SidebarLinkOptions _options;

    /// <summary>
    ///     The sidebar group label.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>null</c>, which places the link outside any group.
    /// </remarks>
    /// <exception cref="ArgumentException"></exception>
    public string? Group
    {
        set => _options.Group = value;
    }

    /// <summary>
    ///     Whether the link opens in a new browser tab.
    /// </summary>
    public bool OpenInNewTab
    {
        set => _options.OpenInNewTab = value;
    }

    /// <summary>
    ///     The link's order within its group.
    /// </summary>
    public int Order
    {
        set => _options.Order = value;
    }

    internal SidebarLinkBuilder(SidebarLinkOptions options) => _options = options;

    /// <summary>
    ///     Shows the link only to authenticated users.
    /// </summary>
    /// <remarks>
    ///     Hiding the link does not protect its destination.
    /// </remarks>
    public SidebarLinkBuilder RequireAuthorization()
    {
        return RequireAuthorization(Array.Empty<IAuthorizeData>());
    }

    /// <summary>
    ///     Shows the link only to users who satisfy an authorization policy.
    /// </summary>
    /// <remarks>
    ///     Hiding the link does not protect its destination.
    /// </remarks>
    /// <param name="policy">The authorization policy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SidebarLinkBuilder RequireAuthorization(AuthorizationPolicy policy)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policy));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy an authorization policy built by a delegate.
    /// </summary>
    /// <remarks>
    ///     Hiding the link does not protect its destination.
    /// </remarks>
    /// <param name="configurePolicy">The delegate that builds the authorization policy.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public SidebarLinkBuilder RequireAuthorization(
        Action<AuthorizationPolicyBuilder> configurePolicy
    )
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(configurePolicy));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy the given authorization data.
    /// </summary>
    /// <remarks>
    ///     Hiding the link does not protect its destination.
    /// </remarks>
    /// <param name="authorizeData">
    ///     The authorization data. When empty, an authenticated user is required.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public SidebarLinkBuilder RequireAuthorization(params IAuthorizeData[] authorizeData)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(authorizeData));

        return this;
    }

    /// <summary>
    ///     Shows the link only to users who satisfy the named authorization policies.
    /// </summary>
    /// <remarks>
    ///     Hiding the link does not protect its destination.
    /// </remarks>
    /// <param name="policyNames">
    ///     The authorization policy names. When empty, an authenticated user is required.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
    public SidebarLinkBuilder RequireAuthorization(params string[] policyNames)
    {
        _options.AuthorizationMetadata.AddRange(AuthorizationMetadata.Create(policyNames));

        return this;
    }
}
