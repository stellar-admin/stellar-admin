using StellarAdmin.Dashboard.Resources;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class EditProductHandler(ProductDataSource source)
    : IResourceEditHandler<EditProductModel>
{
    public async Task<EditProductModel?> FindAsync(string id, CancellationToken cancellationToken)
    {
        var product = await source.FindAsync(id, cancellationToken);

        return product is null
            ? null
            : new(product.Price) { Id = product.Id, ProductName = product.Name };
    }

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        EditProductModel model,
        CancellationToken cancellationToken
    ) => source.UpdateAsync(id, new(model.Id, model.ProductName, model.Price), cancellationToken);
}
