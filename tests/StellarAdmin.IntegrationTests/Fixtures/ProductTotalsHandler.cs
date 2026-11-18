using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductTotalsHandler(ProductState state) : IResourceLayoutHandler<ProductTotal>
{
    public Task<ResourceListResult<ProductTotal>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new ResourceListResult<ProductTotal>(
                [new(request.Layout, request.Scope, request.Search, state.Products.Count)],
                1
            )
        );
}
