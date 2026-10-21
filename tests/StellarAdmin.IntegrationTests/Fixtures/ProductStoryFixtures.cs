using System.ComponentModel.DataAnnotations;
using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductStoryModel
{
    public string Name { get; set; } = "";

    // Rich text by its data type, with no editor configured
    [DataType(DataType.Html)]
    public string? Notes { get; set; }

    public string? Story { get; set; }
}

// Edits product 7's story, and keeps the model each save receives
public sealed class ProductStoryHandler(ProductStories stories)
    : IResourceEditHandler<ProductStoryModel>
{
    public Task<ProductStoryModel?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(
            id == "7"
                ? new ProductStoryModel
                {
                    Name = "Notebook",
                    Notes = "<p>Restock in <em>May</em>.</p>",
                    Story = "<p>Bound in <strong>Kyoto</strong>.</p>",
                }
                : null
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        ProductStoryModel model,
        CancellationToken cancellationToken
    )
    {
        stories.Saved = model;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}

// The model the form saved, and the images a test's StoreImage received
public sealed class ProductStories
{
    public ProductStoryModel? Saved { get; set; }

    public List<string> Stored { get; } = [];
}
