using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

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
    ///     Adds a named resource filter.
    /// </summary>
    public EfCoreResourceScopesBuilder<TEntity> Add(
        string id,
        string title,
        Expression<Func<TEntity, bool>>? predicate = null
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
