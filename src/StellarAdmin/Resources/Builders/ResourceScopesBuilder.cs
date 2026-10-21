using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Resources.Builders;

/// <summary>
///     Configures the scopes that filter a resource's index page.
/// </summary>
/// <typeparam name="TResource">The type of the resource the index lists.</typeparam>
public sealed class ResourceScopesBuilder<TResource> : ResourceScopesBuilderBase<TResource>
{
    internal ResourceScopesBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds a scope, which is shown as a tab that filters the index.
    /// </summary>
    /// <param name="id">
    ///     The identifier passed to the data source when the scope is selected.
    /// </param>
    /// <param name="title">The label of the scope's tab.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Each scope needs a unique identifier, ignoring case. An empty or white space identifier
    ///     or title, or a repeated identifier, is a configuration problem, and the scope is left
    ///     out.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="id" /> or <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    public ResourceScopesBuilder<TResource> Add(string id, string title)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);

        AddScope(new ResourceScopeOptions { Id = id, Title = title });

        return this;
    }
}
