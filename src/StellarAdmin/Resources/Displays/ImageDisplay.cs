using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Displays an image from a stored value: a thumbnail in the grid and a figure on the view page.
/// </summary>
/// <remarks>
///     The value is the image's URL unless <see cref="ResolveUrl(Func{string, string})" /> turns
///     it into one. An empty value shows a placeholder, and an image that fails to load shows a
///     broken-image tile.
/// </remarks>
public sealed class ImageDisplay : FieldDisplay
{
    private readonly ImageUrlResolver _urlResolver = new();

    /// <inheritdoc />
    public override string TemplateName => "Displays/Image";

    /// <inheritdoc />
    /// <remarks>
    ///     Always <c>true</c>: an empty value shows a placeholder.
    /// </remarks>
    public override bool RendersEmptyValue => true;

    /// <summary>
    ///     The image's shape.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageAspectRatio.Square" /> in the grid and
    ///     <see cref="ImageAspectRatio.Original" /> on the view page.
    /// </remarks>
    public ImageAspectRatio? AspectRatio { get; set; }

    /// <summary>
    ///     How the image fills its shape.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageFit.Cover" />.
    /// </remarks>
    public ImageFit Fit { get; set; } = ImageFit.Cover;

    /// <summary>
    ///     What happens when the user clicks or hovers over the image.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageDisplayInteraction.Lightbox" />.
    /// </remarks>
    public ImageDisplayInteraction Interaction { get; set; } = ImageDisplayInteraction.Lightbox;

    /// <summary>
    ///     The image's height in the grid and its maximum height on the view page.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ImageDisplaySize.Small" /> in the grid and
    ///     <see cref="ImageDisplaySize.Medium" /> on the view page.
    /// </remarks>
    public ImageDisplaySize? Size { get; set; }

    /// <summary>
    ///     Turns the stored value into the image's URL.
    /// </summary>
    /// <param name="resolve">Returns the URL, or <c>null</c> when there is no image.</param>
    public void ResolveUrl(Func<string, string?> resolve) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Turns the stored value into the image's URL with the request's services.
    /// </summary>
    /// <param name="resolve">Returns the URL, or <c>null</c> when there is no image.</param>
    /// <remarks>
    ///     The grid calls it once for each row, so it should compute a URL rather than query
    ///     for one.
    /// </remarks>
    public void ResolveUrl(
        Func<string, IServiceProvider, CancellationToken, Task<string?>> resolve
    ) => _urlResolver.Set(resolve);

    // The URL of a stored value, or null for an empty one
    internal Task<string?> ResolveUrlAsync(
        object? value,
        IServiceProvider services,
        CancellationToken cancellationToken
    ) => _urlResolver.ResolveAsync(value, services, cancellationToken);
}
