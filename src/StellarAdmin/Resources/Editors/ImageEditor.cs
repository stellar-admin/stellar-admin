using Microsoft.AspNetCore.Http;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an image editor for a property that stores a reference to an image.
/// </summary>
/// <remarks>
///     A picked file uploads straight away to the delegate set with <see cref="StoreImage" />.
///     The form then saves the value that delegate returns. StellarAdmin never deletes images, so
///     files that are replaced, removed or never saved stay where they were stored.
/// </remarks>
public sealed class ImageEditor : FieldEditor, IFieldEditor<ImageEditorHandler>
{
    private readonly ImageUrlResolver _urlResolver = new();

    /// <summary>
    ///     Gets or sets the image types the editor accepts.
    /// </summary>
    /// <value>
    ///     The media types, such as <c>image/png</c>. The default is JPEG, PNG, WebP, GIF and AVIF.
    /// </value>
    /// <remarks>
    ///     The list must not be empty. The server recognizes the default formats by their
    ///     content, not only by the type the browser declares.
    /// </remarks>
    public IReadOnlyList<string> AcceptedTypes { get; set; } =
    ["image/jpeg", "image/png", "image/webp", "image/gif", "image/avif"];

    /// <summary>
    ///     Gets the additional CSS classes for the parts of the editor.
    /// </summary>
    public override ImageEditorClassNames ClassNames { get; } = new();

    /// <summary>
    ///     Gets or sets how the editor is laid out on a form.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="ImageEditorLayout.Dropzone" />.
    /// </value>
    /// <remarks>
    ///     In an inline row, the editor is always a thumbnail button.
    /// </remarks>
    public ImageEditorLayout Layout { get; set; } = ImageEditorLayout.Dropzone;

    /// <summary>
    ///     Gets or sets the largest file the editor accepts.
    /// </summary>
    /// <value>
    ///     The size in bytes. The default is 5 MB.
    /// </value>
    /// <remarks>
    ///     The value must be positive. The upload request's body limit is raised to allow it.
    /// </remarks>
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;

    /// <inheritdoc />
    public override string TemplateName => "Editors/Image";

    internal bool HasStore => StoreImageAsync is not null;

    internal Func<IFormFile, IServiceProvider, CancellationToken, Task<string>>? StoreImageAsync
    {
        get;
        private set;
    }

    /// <summary>
    ///     Sets a delegate that turns the stored value into the image's URL.
    /// </summary>
    /// <param name="resolve">
    ///     A delegate that returns the URL, or <see langword="null" /> when there is no image.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resolve" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the stored value is the URL. An empty value has no image.
    /// </remarks>
    public void ResolveUrl(Func<string, string?> resolve) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Sets a delegate that turns the stored value into the image's URL with the request's
    ///     services.
    /// </summary>
    /// <param name="resolve">
    ///     A delegate that returns the URL, or <see langword="null" /> when there is no image.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resolve" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     By default, the stored value is the URL. An empty value has no image.
    /// </remarks>
    public void ResolveUrl(
        Func<string, IServiceProvider, CancellationToken, Task<string?>> resolve
    ) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Sets a delegate that saves an uploaded image.
    /// </summary>
    /// <param name="store">
    ///     A delegate that saves the file and returns the value the form saves, such as a file
    ///     name or a URL.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="store" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     An editable image field requires this delegate. The file has already passed the size
    ///     and type checks. The delegate must not return an empty value.
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
