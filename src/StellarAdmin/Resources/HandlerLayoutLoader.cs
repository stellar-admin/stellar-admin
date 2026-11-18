using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StellarAdmin.Resources;

// Registers the handler, and creates the loader that lists a layout's items with it
internal static class HandlerLayoutLoader
{
    public static ResourceLayoutLoader Create<TItem, THandler>(IServiceCollection services)
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        services.TryAddScoped<THandler>();

        return (provider, request, urls, cancellationToken) =>
            CreateModelAsync(
                provider.GetRequiredService<THandler>().ListAsync(request, cancellationToken),
                request,
                urls
            );
    }

    public static ResourceTableLoader CreateTable<TItem, THandler>(IServiceCollection services)
        where THandler : class, IResourceLayoutHandler<TItem>
    {
        services.TryAddScoped<THandler>();

        return (provider, request, cancellationToken) =>
            ListRowsAsync(
                provider.GetRequiredService<THandler>().ListAsync(request, cancellationToken)
            );
    }

    public static async Task<object> CreateModelAsync<TItem>(
        Task<ResourceListResult<TItem>> list,
        ResourceListRequest request,
        ResourceLayoutUrls urls
    ) =>
        new ResourceLayoutModel<TItem>(
            (await list).Items,
            request,
            item => item is null ? null : urls.ViewUrl(item),
            item => item is null ? null : urls.EditUrl(item),
            urls.IndexUrl
        );

    public static async Task<ResourceListResult<object>> ListRowsAsync<TItem>(
        Task<ResourceListResult<TItem>> list
    )
    {
        var result = await list;

        return new(result.Items.Cast<object>().ToArray(), result.TotalCount);
    }
}
