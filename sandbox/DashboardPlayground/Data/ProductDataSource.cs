using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class ProductDataSource : IResourceCrudDataSource<Product>
{
    private readonly Lock _lock = new();
    private readonly List<Product> _products =
    [
        new()
        {
            Id = 1,
            Name = "Notebook",
            Price = 8.50m,
        },
        new()
        {
            Id = 2,
            Name = "Desk lamp",
            Price = 34.95m,
        },
        new()
        {
            Id = 3,
            Name = "Travel mug",
            Price = 18.00m,
        },
        .. Enumerable
            .Range(4, 34)
            .Select(id => new Product
            {
                Id = id,
                Name = $"Sample product {id:00}",
                Price = id * 2.5m,
            }),
    ];

    public Task<ResourceOperationResult> CreateAsync(
        Product resource,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            resource.Id = _products.Select(product => product.Id).DefaultIfEmpty().Max() + 1;
            _products.Add(resource);
        }

        return Task.FromResult(ResourceOperationResult.Success());
    }

    public Task<ResourceOperationResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult(
                int.TryParse(id, out var key)
                && _products.RemoveAll(product => product.Id == key) > 0
                    ? ResourceOperationResult.Success()
                    : ResourceOperationResult.NotFound()
            );
        }
    }

    public Task<Product?> FindAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var product = int.TryParse(id, out var key)
                ? _products.Find(product => product.Id == key)
                : null;

            return Task.FromResult(
                product is null
                    ? null
                    : new Product
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.Price,
                    }
            );
        }
    }

    public Task<ResourceListResult<Product>> ListAsync(
        ResourceListRequest request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            IEnumerable<Product> items = _products.OrderBy(item => item.Id);
            if (request.Paging is { } paging)
            {
                items = items.Skip((paging.Page - 1) * paging.PageSize).Take(paging.PageSize);
            }

            return Task.FromResult(
                new ResourceListResult<Product>(items.ToArray(), _products.Count)
            );
        }
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        Product resource,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            var index = int.TryParse(id, out var key)
                ? _products.FindIndex(product => product.Id == key)
                : -1;
            if (index < 0)
            {
                return Task.FromResult(ResourceOperationResult.NotFound());
            }

            _products[index] = new Product
            {
                Id = key,
                Name = resource.Name,
                Price = resource.Price,
            };

            return Task.FromResult(ResourceOperationResult.Success());
        }
    }
}
