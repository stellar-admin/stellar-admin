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

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.DeleteCalls++;

        return Task.FromResult(
            int.TryParse(id, out var key)
                && state.Products.RemoveAll(product => product.Id == key) > 0
        );
    }

    public Task<Product?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = int.TryParse(id, out var key)
            ? state.Products.Find(product => product.Id == key)
            : null;

        return Task.FromResult(
            product is null ? null : new Product(product.Id, product.Name, product.Price)
        );
    }

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Requests.Add(_id);

        return Task.FromResult<IReadOnlyList<Product>>(state.Products);
    }

    public Task<bool> UpdateAsync(string id, Product resource, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.UpdateCalls++;
        state.SubmittedId = resource.Id;
        var index = int.TryParse(id, out var key)
            ? state.Products.FindIndex(product => product.Id == key)
            : -1;
        if (index < 0 || state.DisappearOnUpdate)
        {
            return Task.FromResult(false);
        }

        state.Products[index] = new(key, resource.Name, resource.Price);

        return Task.FromResult(true);
    }
}
