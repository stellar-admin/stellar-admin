using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StellarAdmin.Resources;

namespace StellarAdmin.EntityFrameworkCore;

// Registers the handler, and creates the loader that reshapes the entities the scope and search select with it
internal static class EfCoreLayoutLoader
{
    public static ResourceLayoutLoader Create<TEntity, TItem, THandler>(IServiceCollection services)
        where TEntity : class
        where THandler : class, IEfCoreLayoutHandler<TEntity, TItem>
    {
        services.TryAddScoped<THandler>();

        return (provider, request, indexUrl, cancellationToken) =>
            HandlerLayoutLoader.CreateModelAsync(
                provider
                    .GetRequiredService<THandler>()
                    .ListAsync(
                        provider
                            .GetRequiredService<EfCoreResourceDataSource<TEntity>>()
                            .Narrow(request),
                        request,
                        cancellationToken
                    ),
                request,
                indexUrl
            );
    }
}
