using StellarAdmin.Resources.Options;

namespace StellarAdmin.EntityFrameworkCore;

// The EF Core settings of a resource that aren't part of its admin panel options
internal sealed class EfCoreResourceOptions<TEntity>
    where TEntity : class
{
    // The context UseDbContext pins; otherwise, the one that maps the entity is found
    internal Type? ContextType { get; set; }

    // Where the create and edit forms' models hold the entity, when the EF builders configured them
    internal EfCoreFormEntity? CreateEntity { get; set; }

    internal EfCoreFormEntity? EditEntity { get; set; }

    // Where a form's model holds the entity: as configured for the form's model, the model itself when it's the entity,
    // or null for a model of the app's own handler
    internal static EfCoreFormEntity? Find(
        ResourceFormOptions? form,
        EfCoreFormEntity? configured
    ) =>
        form switch
        {
            null => null,
            ResourceCreateOptions { ModelType: var type } when configured?.ModelType == type =>
                configured,
            ResourceEditOptions { ModelType: var type } when configured?.ModelType == type =>
                configured,
            ResourceCreateOptions { ModelType: var type } when type == typeof(TEntity) =>
                EfCoreFormEntity.Self(typeof(TEntity)),
            ResourceEditOptions { ModelType: var type } when type == typeof(TEntity) =>
                EfCoreFormEntity.Self(typeof(TEntity)),
            _ => null,
        };
}
