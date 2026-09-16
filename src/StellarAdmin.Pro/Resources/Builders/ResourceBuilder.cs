using StellarAdmin.Pro.Resources.Options;

namespace StellarAdmin.Pro.Resources.Builders;

/// <summary>
///     Configures the management screens of one entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class ResourceBuilder<TEntity>
    where TEntity : class
{
    private readonly ResourceOptions<TEntity> _options;

    public ResourceBuilder(ResourceOptions<TEntity> options)
    {
        _options = options;
    }

    /// <summary>Configures the create page.</summary>
    public ResourceBuilder<TEntity> Create(Action<CreatePageBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new CreatePageBuilder<TEntity>(_options.CreatePage));

        return this;
    }

    /// <summary>Configures the edit page.</summary>
    public ResourceBuilder<TEntity> Edit(Action<EditPageBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new EditPageBuilder<TEntity>(_options.EditPage));

        return this;
    }

    /// <summary>Configures the index page.</summary>
    public ResourceBuilder<TEntity> Index(Action<IndexPageBuilder<TEntity>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        configure(new IndexPageBuilder<TEntity>(_options.IndexPage));

        return this;
    }
}
