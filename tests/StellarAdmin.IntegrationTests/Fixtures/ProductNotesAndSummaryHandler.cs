using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

// Loads two models, so a row detail can't tell which one its partial view takes
public sealed class ProductNotesAndSummaryHandler
    : IResourceRowDetailHandler<ProductNotes>,
        IResourceRowDetailHandler<Product>
{
    Task<ProductNotes?> IResourceRowDetailHandler<ProductNotes>.FindAsync(
        string id,
        CancellationToken cancellationToken
    ) => Task.FromResult<ProductNotes?>(null);

    Task<Product?> IResourceRowDetailHandler<Product>.FindAsync(
        string id,
        CancellationToken cancellationToken
    ) => Task.FromResult<Product?>(null);
}
