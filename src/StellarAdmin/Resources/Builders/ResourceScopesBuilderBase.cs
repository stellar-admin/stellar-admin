using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Provides the members shared by the builders that configure a resource's scopes.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public abstract class ResourceScopesBuilderBase<TResource>
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
    ///     The identifier must match an added scope.
    /// </remarks>
    public string? DefaultScope
    {
        set
        {
            var source = ConfigurationSource.Capture();
            _services.Configure<ResourceOptions<TResource>>(options =>
            {
                options.Index.Scopes!.DefaultScope = value;
                options.Index.Scopes.DefaultScopeSource = source;
            });
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ResourceScopesBuilderBase{TResource}" />
    ///     class and enables scopes on the index.
    /// </summary>
    /// <param name="services">The services that hold the resource's options.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="services" /> is <see langword="null" />.
    /// </exception>
    protected ResourceScopesBuilderBase(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        _services = services;
        _services.Configure<ResourceOptions<TResource>>(options => options.Index.Scopes = new());
    }

    /// <summary>
    ///     Adds a scope, which is shown as a tab that filters the index.
    /// </summary>
    /// <param name="scope">The scope, which the data source reads.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="scope" /> is <see langword="null" />.
    /// </exception>
    protected void AddScope(ResourceScopeOptions scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(scope.Id, nameof(scope));
        ArgumentNullException.ThrowIfNull(scope.Title, nameof(scope));

        scope.Source = ConfigurationSource.Capture();
        _services.Configure<ResourceOptions<TResource>>(options =>
            options.Index.Scopes!.MutableItems.Add(scope)
        );
    }
}
