using StellarAdmin.Resources;

namespace StellarAdmin.IntegrationTests.Fixtures;

public sealed class ProductPhotoModel
{
    public string Name { get; set; } = "";

    public string? Photo { get; set; }

    public string? Thumbnail { get; set; }
}

// Edits product 7's photo, and keeps the model each save receives
public sealed class ProductPhotoHandler(ProductPhotos photos)
    : IResourceEditHandler<ProductPhotoModel>
{
    public Task<ProductPhotoModel?> FindAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(
            id == "7"
                ? new ProductPhotoModel
                {
                    Name = "Notebook",
                    Photo = "notebook.jpg",
                    Thumbnail = "notebook-small.jpg",
                }
                : null
        );

    public Task<ResourceOperationResult> UpdateAsync(
        string id,
        ProductPhotoModel model,
        CancellationToken cancellationToken
    )
    {
        photos.Saved = model;

        return Task.FromResult(ResourceOperationResult.Success());
    }
}

// The files a test's StoreImage received, and the model the form saved
public sealed class ProductPhotos
{
    public ProductPhotoModel? Saved { get; set; }

    public List<StoredPhoto> Stored { get; } = [];
}

public sealed record StoredPhoto(string FileName, string ContentType, long Length);
