using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the scopes that filter the index page of a resource that EF Core loads.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public sealed class EfCoreResourceScopesBuilder<TEntity> : ResourceScopesBuilderBase<TEntity>
    where TEntity : class
{
    internal EfCoreResourceScopesBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds a scope that shows every resource.
    /// </summary>
    /// <param name="id">The identifier of the scope.</param>
    /// <param name="title">The label of the scope's tab.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="id" /> or <paramref name="title" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Each scope needs a unique identifier, ignoring case. An empty or white space identifier
    ///     or title, or a repeated identifier, is a configuration problem, and the scope is left
    ///     out.
    /// </remarks>
    public EfCoreResourceScopesBuilder<TEntity> Add(string id, string title) =>
        AddScope(id, title, null);

    /// <summary>
    ///     Adds a scope that shows the resources that match a predicate.
    /// </summary>
    /// <param name="id">The identifier of the scope.</param>
    /// <param name="title">The label of the scope's tab.</param>
    /// <param name="predicate">An expression that filters the resources.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="id" />, <paramref name="title" /> or <paramref name="predicate" /> is
    ///     <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     Each scope needs a unique identifier, ignoring case. An empty or white space identifier
    ///     or title, or a repeated identifier, is a configuration problem, and the scope is left
    ///     out.
    /// </remarks>
    public EfCoreResourceScopesBuilder<TEntity> Add(
        string id,
        string title,
        Expression<Func<TEntity, bool>> predicate
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return AddScope(id, title, predicate);
    }

    private EfCoreResourceScopesBuilder<TEntity> AddScope(
        string id,
        string title,
        Expression<Func<TEntity, bool>>? predicate
    )
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(title);

        AddScope(
            new EfCoreResourceScopeOptions<TEntity>
            {
                Id = id,
                Title = title,
                Predicate = predicate,
            }
        );

        return this;
    }
}
