using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class InventoryItemDataSource(List<InventoryItem> items)
    : IResourceDataSource<InventoryItem>
{
    public Task CreateAsync(InventoryItem resource, CancellationToken cancellationToken)
    {
        items.Add(resource);

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task<InventoryItem?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<InventoryItem?>(null);

    public Task<IReadOnlyList<InventoryItem>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<InventoryItem>>(items);

    public Task<bool> UpdateAsync(
        string id,
        InventoryItem resource,
        CancellationToken cancellationToken
    ) => Task.FromResult(false);
}
