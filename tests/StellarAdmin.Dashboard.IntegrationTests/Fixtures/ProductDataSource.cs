using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductDataSource(ProductState state) : IResourceDataSource<Product>
{
    private readonly Guid _id = Guid.NewGuid();

    public Task CreateAsync(Product resource, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.SubmittedId = resource.Id;
        resource.Id = state.Products.Count + 1;
        state.Products.Add(resource);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Requests.Add(_id);

        return Task.FromResult<IReadOnlyList<Product>>(state.Products);
    }
}
