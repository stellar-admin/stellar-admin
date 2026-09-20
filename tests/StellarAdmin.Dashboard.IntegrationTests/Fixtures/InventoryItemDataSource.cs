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

    public Task<IReadOnlyList<InventoryItem>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<InventoryItem>>(items);
}
