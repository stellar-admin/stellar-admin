using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StellarAdmin.Resources;

// Registers the handler, and creates the loader that lists a data layout's items with it
internal static class HandlerDataLayoutLoader
{
    public static ResourceDataLayoutLoader Create<TItem, THandler>(IServiceCollection services)
        where THandler : class, IResourceDataLayoutHandler<TItem>
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
        where THandler : class, IResourceDataLayoutHandler<TItem>
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
        ResourceDataLayoutUrls urls
    ) =>
        new ResourceDataLayoutModel<TItem>(
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
