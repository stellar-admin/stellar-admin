using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class ProductDataSource(ProductState state) : IResourceCrudDataSource<Product>
{
    private readonly Guid _id = Guid.NewGuid();

    public Task<ResourceOperationResult> CreateAsync(
        Product resource,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (state.CreateResult is { } result)
        {
            return Task.FromResult(result);
        }

        state.SubmittedId = resource.Id;
        resource.Id = state.Products.Count + 1;
        state.Products.Add(resource);

        return Task.FromResult(ResourceOperationResult.Success());
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.DeleteCalls++;
        if (state.DeleteResult is { } result)
        {
            return Task.FromResult(result);
        }

        return Task.FromResult(
            int.TryParse(id, out var key)
            && state.Products.RemoveAll(product => product.Id == key) > 0
                ? ResourceOperationResult.Success()
                : ResourceOperationResult.NotFound()
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

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Product resource,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.UpdateCalls++;
        if (state.UpdateResult is { } result)
        {
            return Task.FromResult(result);
        }
        state.SubmittedId = resource.Id;
        var index = int.TryParse(id, out var key)
            ? state.Products.FindIndex(product => product.Id == key)
            : -1;
        if (index < 0 || state.DisappearOnUpdate)
        {
            return Task.FromResult(ResourceOperationResult.NotFound());
        }

        state.Products[index] = new(key, resource.Name, resource.Price);

        return Task.FromResult(ResourceOperationResult.Success());
    }
}
