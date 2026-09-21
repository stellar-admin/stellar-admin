using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductMaintenanceDataSource(ProductDataSource source)
    : IResourceDataSource<Product>,
        IResourceEditHandler<Product>,
        IResourceDeleteHandler<Product>
{
    public Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    ) => source.DeleteAsync(id, cancellationToken);

    public Task<Product?> FindAsync(string id, CancellationToken cancellationToken) =>
        source.FindAsync(id, cancellationToken);

    public Task<ResourceListResult<Product>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => source.ListAsync(request, cancellationToken);

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Product model,
        CancellationToken cancellationToken
    ) => source.UpdateAsync(id, model, cancellationToken);
}
