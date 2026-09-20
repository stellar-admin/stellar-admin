using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class CustomProductDataSource : IResourceDataSource<CustomProduct>
{
    public Task CreateAsync(CustomProduct resource, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<CustomProduct>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<CustomProduct>>([]);
}
