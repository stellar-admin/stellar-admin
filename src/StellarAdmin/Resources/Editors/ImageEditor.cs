using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an image editor for a property that stores a reference to an image.
/// </summary>
/// <remarks>
///     A picked file uploads straight away to <see cref="StoreImage" />, which saves it and
///     returns the value to store; the form then saves only that value. StellarAdmin never
///     deletes images, so files that are replaced, removed or never saved stay where
///     <see cref="StoreImage" /> put them.
/// </remarks>
public sealed class ImageEditor : FieldEditor, IFieldEditor<ImageEditorHandler>
{
    private readonly ImageUrlResolver _urlResolver = new();

    /// <summary>
    ///     The image types the editor accepts, as media types.
    /// </summary>
    /// <remarks>
    ///     Defaults to JPEG, PNG, WebP, GIF and AVIF. The server recognises these formats by
    ///     their content, not only by the type the browser declares.
    /// </remarks>
    public IReadOnlyList<string> AcceptedTypes { get; set; } =
    ["image/jpeg", "image/png", "image/webp", "image/gif", "image/avif"];

    /// <summary>
    ///     Additional CSS classes for the image editor, its image and its note.
    /// </summary>
    public override ImageEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     How the editor is laid out on a form.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageEditorLayout.Dropzone" />. In an inline grid row the
    ///     editor is always a thumbnail button.
    /// </remarks>
    public ImageEditorLayout Layout { get; set; } = ImageEditorLayout.Dropzone;

    /// <summary>
    ///     The largest file the editor accepts, in bytes.
    /// </summary>
    /// <remarks>
    ///     Defaults to 5 MB. The upload request's body limit is raised to allow it.
    /// </remarks>
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;

    internal bool HasStore => StoreImageAsync is not null;

    internal Func<IFormFile, IServiceProvider, CancellationToken, Task<string>>? StoreImageAsync
    {
        get;
        private set;
    }

    /// <summary>
    ///     Turns the stored value into the image's URL.
    /// </summary>
    /// <param name="resolve">Returns the URL, or <c>null</c> when there is no image.</param>
    public void ResolveUrl(Func<string, string?> resolve) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Turns the stored value into the image's URL with the request's services.
    /// </summary>
    /// <param name="resolve">Returns the URL, or <c>null</c> when there is no image.</param>
    public void ResolveUrl(
        Func<string, IServiceProvider, CancellationToken, Task<string?>> resolve
    ) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Saves an uploaded image and returns the value to store in the property.
    /// </summary>
    /// <param name="store">
    ///     Saves the file, which has passed the size and type checks, and returns the value the
    ///     form saves, such as a file name or a URL.
    /// </param>
    /// <remarks>
    ///     Required: the resource's options fail validation without it.
    /// </remarks>
    public void StoreImage(Func<IFormFile, IServiceProvider, CancellationToken, Task<string>> store)
    {
        ArgumentNullException.ThrowIfNull(store);

        StoreImageAsync = store;
    }

    // The URL of a stored value, or null for an empty one
    internal Task<string?> ResolveUrlAsync(
        object? value,
        IServiceProvider services,
        CancellationToken cancellationToken
    ) => _urlResolver.ResolveAsync(value, services, cancellationToken);
}
