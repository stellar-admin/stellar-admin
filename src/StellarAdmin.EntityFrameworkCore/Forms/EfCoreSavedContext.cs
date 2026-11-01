using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides the record that a create or edit form saved.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
public sealed class EfCoreSavedContext<TEntity, TModel> : EfCoreFormContext<TEntity, TModel>
    where TEntity : class
{
    internal EfCoreSavedContext(
        DbContext db,
        TEntity entity,
        TModel model,
        IServiceProvider services
    )
        : base(db, entity, model, services) { }
}
