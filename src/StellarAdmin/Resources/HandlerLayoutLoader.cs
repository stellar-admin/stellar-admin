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

        return (provider, request, indexUrl, cancellationToken) =>
            CreateModelAsync(
                provider.GetRequiredService<THandler>().ListAsync(request, cancellationToken),
                request,
                indexUrl
            );
    }

    public static async Task<object> CreateModelAsync<TItem>(
        Task<ResourceListResult<TItem>> list,
        ResourceListRequest request,
        Func<object?, string> indexUrl
    ) =>
        new ResourceLayoutModel<TItem>((await list).Items, request, _ => null, _ => null, indexUrl);
}
