using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class CustomProductDataSource : IResourceDataSource<CustomProduct>
{
    public Task CreateAsync(CustomProduct resource, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(false);

    public Task<CustomProduct?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult<CustomProduct?>(id == "item-1" ? new() : null);

    public Task<IReadOnlyList<CustomProduct>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CustomProduct>>([]);

    public Task<bool> UpdateAsync(
        string id,
        CustomProduct resource,
        CancellationToken cancellationToken
    ) => Task.FromResult(false);
}
