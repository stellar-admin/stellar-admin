using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class ProductDataSource : IResourceDataSource<Product>
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
    ];

    public Task CreateAsync(Product resource, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            resource.Id = _products.Max(product => product.Id) + 1;
            _products.Add(resource);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Product>>(_products.ToArray());
        }
    }
}
