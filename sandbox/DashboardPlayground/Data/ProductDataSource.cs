using DashboardPlayground.Models;
using StellarAdmin.Dashboard.Resources;

namespace DashboardPlayground.Data;

public sealed class ProductDataSource : IResourceDataSource<Product>
{
    private readonly IReadOnlyList<Product> _products =
    [
        new(1, "Notebook", 8.50m),
        new(2, "Desk lamp", 34.95m),
        new(3, "Travel mug", 18.00m),
    ];

    public Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_products);
    }
}
