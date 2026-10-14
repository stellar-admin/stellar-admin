using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductFactsDataSource(List<ProductFactsModel> items)
    : IResourceDataSource<ProductFactsModel>
{
    public Task<ResourceListResult<ProductFactsModel>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResourceListResult<ProductFactsModel>(items.ToArray(), items.Count));
}
