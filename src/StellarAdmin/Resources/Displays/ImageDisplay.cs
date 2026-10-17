using StellarAdmin.UI;

namespace StellarAdmin.Resources.Displays;

/// <summary>
///     Shows an image as a thumbnail in the grid and as a figure on the view page.
/// </summary>
/// <remarks>
///     The value is the image's URL, unless <see cref="ResolveUrl(Func{string, string})" /> turns
///     it into one. An empty value shows a placeholder. An image that fails to load shows a
///     broken-image tile.
/// </remarks>
public sealed class ImageDisplay : FieldDisplay
{
    private readonly ImageUrlResolver _urlResolver = new();

    /// <inheritdoc />
    public override string TemplateName => "Displays/Image";

    /// <inheritdoc />
    /// <value>
    ///     Always <see langword="true" />, because an empty value shows a placeholder.
    /// </value>
    public override bool RendersEmptyValue => true;

    /// <summary>
    ///     Gets or sets the shape of the image.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the shape, or <see langword="null" /> for
    ///     the default. The default is <see cref="ImageAspectRatio.Square" /> in the grid and
    ///     <see cref="ImageAspectRatio.Original" /> on the view page.
    /// </value>
    public ImageAspectRatio? AspectRatio { get; set; }

    /// <summary>
    ///     Gets or sets how the image fills its shape.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the fit. The default is
    ///     <see cref="ImageFit.Cover" />.
    /// </value>
    public ImageFit Fit { get; set; } = ImageFit.Cover;

    /// <summary>
    ///     Gets or sets what happens when the user clicks or hovers over the image.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the interaction. The default is
    ///     <see cref="ImageDisplayInteraction.Lightbox" />.
    /// </value>
    public ImageDisplayInteraction Interaction { get; set; } = ImageDisplayInteraction.Lightbox;

    /// <summary>
    ///     Gets or sets the image's height in the grid and its maximum height on the view page.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size, or <see langword="null" /> for
    ///     the default. The default is <see cref="ImageDisplaySize.Small" /> in the grid and
    ///     <see cref="ImageDisplaySize.Medium" /> on the view page.
    /// </value>
    public ImageDisplaySize? Size { get; set; }

    /// <summary>
    ///     Sets how the stored value is turned into the image's URL.
    /// </summary>
    /// <param name="resolve">
    ///     A delegate that returns the URL for a stored value, or <see langword="null" /> when
    ///     there is no image.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resolve" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The delegate isn't called for an empty value.
    /// </remarks>
    public void ResolveUrl(Func<string, string?> resolve) => _urlResolver.Set(resolve);

    /// <summary>
    ///     Sets how the stored value is turned into the image's URL, with the request's services.
    /// </summary>
    /// <param name="resolve">
    ///     A delegate that returns the URL for a stored value, or <see langword="null" /> when
    ///     there is no image.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="resolve" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The delegate isn't called for an empty value. The grid calls it once for each row, so it
    ///     should compute a URL rather than query for one.
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
