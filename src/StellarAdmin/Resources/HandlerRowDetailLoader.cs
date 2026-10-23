using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StellarAdmin.Resources;

internal sealed class HandlerRowDetailLoader<TModel, THandler> : ResourceRowDetailLoader
    where THandler : class, IResourceRowDetailHandler<TModel>
{
    public override async Task<object?> LoadAsync(
        IServiceProvider services,
        string key,
        CancellationToken cancellationToken
    ) => await services.GetRequiredService<THandler>().FindAsync(key, cancellationToken);
}

// Creates the loader for a handler, whose model is the one its IResourceRowDetailHandler<TModel> names
internal static class HandlerRowDetailLoader
{
    public static ResourceRowDetailLoader Create(Type handlerType, IServiceCollection services)
    {
        var models = handlerType
            .GetInterfaces()
            .Where(type =>
                type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IResourceRowDetailHandler<>)
            )
            .Select(type => type.GetGenericArguments()[0])
            .ToArray();
        if (models.Length != 1)
        {
            return new UnusableHandlerRowDetailLoader(handlerType, models);
        }

        services.TryAddScoped(handlerType);

        return (ResourceRowDetailLoader)
            Activator.CreateInstance(
                typeof(HandlerRowDetailLoader<,>).MakeGenericType(models[0], handlerType)
            )!;
    }
}

// Stands in for a handler that names no model, or several, until the resource's options are checked
internal sealed class UnusableHandlerRowDetailLoader(Type handlerType, IReadOnlyList<Type> models)
    : ResourceRowDetailLoader
{
    public Type HandlerType => handlerType;

    public IReadOnlyList<Type> Models => models;

    public override Task<object?> LoadAsync(
        IServiceProvider services,
        string key,
        CancellationToken cancellationToken
    ) =>
        throw new InvalidOperationException(
            $"{handlerType.Name} can't load a row detail because it doesn't implement IResourceRowDetailHandler<TModel> for exactly one model."
        );
}
