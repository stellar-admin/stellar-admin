using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StellarAdmin.Resources;

// Registers the handler, and creates the loader that finds a row detail's model with it
internal static class HandlerRowDetailLoader
{
    public static Func<IServiceProvider, string, CancellationToken, Task<object?>> Create<
        TModel,
        THandler
    >(IServiceCollection services)
        where THandler : class, IResourceRowDetailHandler<TModel>
    {
        services.TryAddScoped<THandler>();

        return async (provider, key, cancellationToken) =>
            await provider.GetRequiredService<THandler>().FindAsync(key, cancellationToken);
    }
}
