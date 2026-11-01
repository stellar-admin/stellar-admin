using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Resources.Builders;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Configures the edit page of a resource that EF Core saves.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the edit form binds to.</typeparam>
public sealed class EfCoreResourceEditBuilder<TEntity, TModel> : ResourceEditBuilder<TModel>
    where TEntity : class
{
    private readonly IServiceCollection _services;

    internal EfCoreResourceEditBuilder(
        IServiceCollection services,
        ResourceEditBuilder<TModel> builder,
        EfCoreFormEntity entity
    )
        : base(builder)
    {
        _services = services;
        services.Configure<EfCoreFormOptions<EfCoreEditEvents<TEntity, TModel>>>(options =>
        {
            options.Entity = entity;
            options.Events = new();
            options.EventsType = null;
            options.HasEvents = false;
        });

        if (entity.Property is { } property)
        {
            UseResourceProperty(property.Name);
        }
    }

    /// <summary>
    ///     Configures the events that run when the edit form loads and saves a record.
    /// </summary>
    /// <param name="configure">A delegate that sets the events' delegates.</param>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An events class set with <see cref="UseEvents{TEvents}" /> replaces these delegates.
    /// </remarks>
    public EfCoreResourceEditBuilder<TEntity, TModel> Events(
        Action<EfCoreEditEvents<TEntity, TModel>> configure
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        _services.Configure<EfCoreFormOptions<EfCoreEditEvents<TEntity, TModel>>>(options =>
        {
            options.HasEvents = true;
            configure(options.Events);
        });

        return this;
    }

    /// <summary>
    ///     Runs the edit form's events from a class that the request's services create.
    /// </summary>
    /// <typeparam name="TEvents">The type of the events class.</typeparam>
    /// <returns>The builder, so that calls can be chained.</returns>
    /// <remarks>
    ///     Registers <typeparamref name="TEvents" /> as a scoped service if it isn't registered
    ///     already, so its constructor can take services.
    /// </remarks>
    public EfCoreResourceEditBuilder<TEntity, TModel> UseEvents<TEvents>()
        where TEvents : EfCoreEditEvents<TEntity, TModel>
    {
        _services.TryAddScoped<TEvents>();
        _services.Configure<EfCoreFormOptions<EfCoreEditEvents<TEntity, TModel>>>(options =>
        {
            options.EventsType = typeof(TEvents);
            options.HasEvents = true;
        });

        return this;
    }
}
