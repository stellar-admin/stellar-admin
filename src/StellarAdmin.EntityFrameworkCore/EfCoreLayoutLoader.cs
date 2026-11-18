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

        return (provider, request, urls, cancellationToken) =>
            HandlerLayoutLoader.CreateModelAsync(
                ListAsync<TEntity, TItem, THandler>(provider, request, cancellationToken),
                request,
                urls
            );
    }

    public static ResourceTableLoader CreateTable<TEntity, TItem, THandler>(
        IServiceCollection services
    )
        where TEntity : class
        where THandler : class, IEfCoreLayoutHandler<TEntity, TItem>
    {
        services.TryAddScoped<THandler>();

        return (provider, request, cancellationToken) =>
            HandlerLayoutLoader.ListRowsAsync(
                ListAsync<TEntity, TItem, THandler>(provider, request, cancellationToken)
            );
    }

    private static Task<ResourceListResult<TItem>> ListAsync<TEntity, TItem, THandler>(
        IServiceProvider provider,
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
        where TEntity : class
        where THandler : class, IEfCoreLayoutHandler<TEntity, TItem> =>
        provider
            .GetRequiredService<THandler>()
            .ListAsync(
                provider.GetRequiredService<EfCoreResourceDataSource<TEntity>>().Narrow(request),
                request,
                cancellationToken
            );
}
