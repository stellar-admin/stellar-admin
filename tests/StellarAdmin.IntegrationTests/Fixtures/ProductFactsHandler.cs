using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductFactsHandler(ProductDataSource source, ProductFactsChange change)
    : IResourceEditHandler<ProductFactsModel>
{
    public static readonly Guid Reference = new("0b9c4f1e-6d2a-4c8e-9f3b-2a7d5e1c8b40");

    public async Task<ProductFactsModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        var product = await source.FindAsync(id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var model = new ProductFactsModel
        {
            Active = true,
            Code = "NB-7",
            Discontinued = false,
            Email = "sales@voyager.example",
            Notes = "Ships from Cape Town.\nAllow two days.",
            Opens = new(9, 30),
            Phone = "+27 21 555 0100",
            Photo = "notebook.jpg",
            Price = product.Price,
            ProductName = product.Name,
            Reference = Reference,
            Released = new(2026, 3, 14),
            Status = ProductFactsStatus.OnSale,
            Updated = new(2026, 10, 12, 14, 5, 0),
            Website = "https://voyager.example/notebook",
            Weight = 1.5m,
        };
        change.Apply(model);

        return model;
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        ProductFactsModel model,
        CancellationToken cancellationToken
    ) => Task.FromResult(ResourceOperationResult.Success());
}

// Changes the facts a test reads
public sealed record ProductFactsChange(Action<ProductFactsModel> Apply);
