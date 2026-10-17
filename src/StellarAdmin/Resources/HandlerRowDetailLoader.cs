using Microsoft.Extensions.DependencyInjection;

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
