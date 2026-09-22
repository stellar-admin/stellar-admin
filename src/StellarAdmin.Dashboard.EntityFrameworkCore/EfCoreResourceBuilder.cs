using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource.
/// </summary>
public sealed class EfCoreResourceBuilder<TContext, TEntity>
    where TContext : DbContext
    where TEntity : class
{
    private readonly ResourceBuilder<TEntity> _resource;
    private readonly ResourceIndexBuilder<TEntity> _index;

    /// <summary>
    ///     The plural resource label.
    /// </summary>
    public string PluralLabel
    {
        set => _resource.PluralLabel = value;
    }

    /// <summary>
    ///     The singular resource label.
    /// </summary>
    public string SingularLabel
    {
        set => _resource.SingularLabel = value;
    }

    internal EfCoreResourceBuilder(
        ResourceBuilder<TEntity> resource,
        ResourceIndexBuilder<TEntity> index
    )
    {
        _resource = resource;
        _index = index;
    }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    public EfCoreResourceBuilder<TContext, TEntity> Index(
        Action<EfCoreResourceIndexBuilder<TContext, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(_index));

        return this;
    }
}
