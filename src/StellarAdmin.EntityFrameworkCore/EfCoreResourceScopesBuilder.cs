using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures EF Core resource index scopes.
/// </summary>
public sealed class EfCoreResourceScopesBuilder<TEntity>
    where TEntity : class
{
    private readonly IServiceCollection _services;

    /// <summary>
    ///     The scope applied when no recognized scope is requested.
    /// </summary>
    public string? DefaultScope
    {
        set =>
            _services.Configure<ResourceOptions<TEntity>>(options =>
                options.Index.Scopes!.DefaultScope = value
            );
    }

    internal EfCoreResourceScopesBuilder(IServiceCollection services) => _services = services;

    /// <summary>
    ///     Adds a named resource filter that shows every record.
    /// </summary>
    public EfCoreResourceScopesBuilder<TEntity> Add(string id, string title) =>
        AddScope(id, title, null);

    /// <summary>
    ///     Adds a named resource filter that shows the records matching
    ///     <paramref name="predicate" />.
    /// </summary>
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
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        _services.Configure<ResourceOptions<TEntity>>(options =>
            options.Index.Scopes!.Items.Add(
                new EfCoreResourceScopeOptions<TEntity>(id, title) { Predicate = predicate }
            )
        );

        return this;
    }
}
