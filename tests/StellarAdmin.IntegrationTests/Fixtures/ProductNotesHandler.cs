using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductNotesHandler(ProductState state)
    : IResourceRowDetailHandler<ProductNotes>
{
    public Task<ProductNotes?> FindAsync(string id, CancellationToken cancellationToken)
    {
        var product = state.Products.SingleOrDefault(product => product.Id.ToString() == id);

        return Task.FromResult(
            product is null ? null : new ProductNotes(product.Name, $"Notes for {product.Name}")
        );
    }
}
