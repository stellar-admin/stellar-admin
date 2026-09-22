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

    public Task<ResourceListResult<Product>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        state.Requests.Add(_id);
        state.ListRequests.Add(request);

        IEnumerable<Product> items = state.Products;
        if (request.Search is { } search)
        {
            items = items.Where(item =>
                item.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            );
        }

        var totalCount = items.Count();
        items = request.Sort switch
        {
            { Field: nameof(Product.Name), Direction: ResourceSortDirection.Descending } => items
                .OrderByDescending(item => item.Name)
                .ThenBy(item => item.Id),
            { Field: nameof(Product.Name) } => items
                .OrderBy(item => item.Name)
                .ThenBy(item => item.Id),
            { Field: nameof(Product.Price), Direction: ResourceSortDirection.Descending } => items
                .OrderByDescending(item => item.Price)
                .ThenBy(item => item.Id),
            { Field: nameof(Product.Price) } => items
                .OrderBy(item => item.Price)
                .ThenBy(item => item.Id),
            _ => items.OrderBy(item => item.Id),
        };
        if (request.Paging is { } paging)
        {
            items = items.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);
        }

        return Task.FromResult(new ResourceListResult<Product>(items.ToArray(), totalCount));
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
