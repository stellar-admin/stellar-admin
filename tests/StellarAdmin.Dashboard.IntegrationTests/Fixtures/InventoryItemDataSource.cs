using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class InventoryItemDataSource(List<InventoryItem> items)
    : IResourceCrudDataSource<InventoryItem>
{
    public Task<ResourceOperationResult> CreateAsync(
        InventoryItem resource,
        CancellationToken cancellationToken
    )
    {
        items.Add(resource);

        return Task.FromResult(ResourceOperationResult.Success());
    }

    public Task<ResourceOperationResult> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.NotFound());

    public Task<InventoryItem?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<InventoryItem?>(null);

    public Task<ResourceListResult<InventoryItem>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult(
            new ResourceListResult<InventoryItem>(
                request.Paging is { } paging
                    ? items
                        .Skip((paging.Page - 1) * paging.PageSize)
                        .Take(paging.PageSize)
                        .ToArray()
                    : items.ToArray(),
                items.Count
            )
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        InventoryItem resource,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.NotFound());
}
