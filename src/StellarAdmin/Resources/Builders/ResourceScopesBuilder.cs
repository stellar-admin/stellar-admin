using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the scopes that filter a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceScopesBuilder<TResource>
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     Sets the scope that applies when the request doesn't name a known scope.
    /// </summary>
    /// <value>
    ///     The identifier of the scope, or <see langword="null" /> to apply no scope. The default
    ///     is <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     The identifier must match a scope added with <see cref="Add" />.
    /// </remarks>
    public string? DefaultScope
    {
        set =>
            _services.Configure<ResourceOptions<TResource>>(options =>
                options.Index.Scopes!.DefaultScope = value
            );
    }

    internal ResourceScopesBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a scope, which is shown as a tab that filters the index.
    /// </summary>
    /// <param name="id">
    ///     The identifier passed to the data source when the scope is selected.
    /// </param>
    /// <param name="title">The label of the scope's tab.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Each scope needs a unique identifier, ignoring case.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="id" /> or <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     <paramref name="id" /> or <paramref name="title" /> is empty or whitespace.
    /// </exception>
    public ResourceScopesBuilder<TResource> Add(string id, string title)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Scopes!.Items.Add(new(id, title))
        );

        return this;
    }
}
