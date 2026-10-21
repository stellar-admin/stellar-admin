using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Resources.Editors;

// An editor whose images upload to the resource's upload action as they are picked
internal interface IImageUploadEditor
{
    IReadOnlyList<string> AcceptedTypes { get; }

    bool HasStore { get; }

    long MaxFileSize { get; }

    // The value the form saves and the image's URL
    Task<(string Value, string? Url)> StoreAsync(
        IFormFile file,
        IServiceProvider services,
        CancellationToken cancellationToken
    );
}
