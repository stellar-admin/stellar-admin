using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides the record and services that a create or edit form's events receive.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
public abstract class EfCoreFormContext<TEntity, TModel>
    where TEntity : class
{
    /// <summary>
    ///     Gets the database context that loads and saves the entity.
    /// </summary>
    public DbContext Db { get; }

    /// <summary>
    ///     Gets the entity.
    /// </summary>
    /// <value>
    ///     The entity, which is the same object as <see cref="Model" /> when the form binds the
    ///     entity itself.
    /// </value>
    public TEntity Entity { get; }

    /// <summary>
    ///     Gets the form's model.
    /// </summary>
    public TModel Model { get; }

    /// <summary>
    ///     Gets the request's services.
    /// </summary>
    public IServiceProvider Services { get; }

    private protected EfCoreFormContext(
        DbContext db,
        TEntity entity,
        TModel model,
        IServiceProvider services
    )
    {
        Db = db;
        Entity = entity;
        Model = model;
        Services = services;
    }
}
