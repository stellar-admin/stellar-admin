using Microsoft.EntityFrameworkCore;

namespace StellarAdmin.EntityFrameworkCore;

/// <summary>
///     Provides the record that an edit form loaded.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TModel">The type of the model the form binds to.</typeparam>
/// <remarks>
///     The entity isn't tracked by <see cref="EfCoreFormContext{TEntity, TModel}.Db" />.
/// </remarks>
public sealed class EfCoreLoadedContext<TEntity, TModel> : EfCoreFormContext<TEntity, TModel>
    where TEntity : class
{
    internal EfCoreLoadedContext(
        DbContext db,
        TEntity entity,
        TModel model,
        IServiceProvider services
    )
        : base(db, entity, model, services) { }
}
