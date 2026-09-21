using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class CustomProductDataSource : IResourceCrudDataSource<CustomProduct>
{
    public Task<ResourceOperationResult> CreateAsync(
        CustomProduct resource,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());

    public Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.NotFound());

    public Task<CustomProduct?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<CustomProduct?>(id == "item-1" ? new() : null);

    public Task<ResourceListResult<CustomProduct>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) => Task.FromResult(new ResourceListResult<CustomProduct>([], 0));

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        CustomProduct resource,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.NotFound());
}
