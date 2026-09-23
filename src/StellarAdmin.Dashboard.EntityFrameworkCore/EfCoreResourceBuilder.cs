using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Dashboard.Resources;
using StellarAdmin.Dashboard.Resources.Builders;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

/// <summary>
///     Configures an EF Core resource.
/// </summary>
public sealed class EfCoreResourceBuilder<TContext, TEntity>
    : ResourceBuilderBase<TEntity, EfCoreResourceBuilder<TContext, TEntity>>
    where TContext : DbContext
    where TEntity : class
{
    internal EfCoreResourceBuilder(IServiceCollection services)
        : base(services) { }

    /// <summary>
    ///     Adds an EF Core reference for a foreign-key field.
    /// </summary>
    public EfCoreResourceBuilder<TContext, TEntity> AddReference<TKey, TTarget>(
        Expression<Func<TEntity, TKey>> key,
        Expression<Func<TEntity, TTarget?>> navigation,
        Expression<Func<TTarget, string>> display
    )
        where TTarget : class
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(display);

        var keyProperties = ResourcePropertyPath.GetProperties(key);
        var navigationProperties = ResourcePropertyPath.GetProperties(navigation);
        var displayProperties = ResourcePropertyPath.GetProperties(display);
        if (
            keyProperties is not { Length: 1 }
            || navigationProperties is not { Length: 1 }
            || displayProperties is not { Length: 1 }
            || displayProperties[0].PropertyType != typeof(string)
        )
        {
            throw new ArgumentException("Reference selectors must select direct properties.");
        }

        var reference = new EfCoreReference<TEntity, TTarget>(
            keyProperties[0].Name,
            navigationProperties[0].Name,
            navigation,
            display
        );
        Services.Configure<EfCoreResourceReferences<TEntity>>(options =>
        {
            if (options.Items.Any(item => item.FieldName == reference.FieldName))
            {
                throw new InvalidOperationException(
                    $"Reference '{reference.FieldName}' is already registered."
                );
            }

            options.Items.Add(reference);
        });

        return this;
    }

    /// <summary>
    ///     Configures the index page.
    /// </summary>
    public EfCoreResourceBuilder<TContext, TEntity> Index(
        Action<EfCoreResourceIndexBuilder<TContext, TEntity>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(new(Services));

        return this;
    }
}
