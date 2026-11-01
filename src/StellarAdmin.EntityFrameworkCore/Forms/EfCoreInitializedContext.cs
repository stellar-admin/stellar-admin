using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides the new record that a create form is about to show.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
public sealed class EfCoreInitializedContext<TEntity, TModel> : EfCoreFormContext<TEntity, TModel>
    where TEntity : class
{
    internal EfCoreInitializedContext(
        DbContext db,
        TEntity entity,
        TModel model,
        IServiceProvider services
    )
        : base(db, entity, model, services) { }
}
