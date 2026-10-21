using Microsoft.AspNetCore.Http;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Configures an editor for formatted text, such as headings, lists, links and tables, that
///     the property stores as HTML.
/// </summary>
/// <remarks>
///     The HTML is sanitized with <see cref="IRichTextSanitizer" /> when the form is saved. Images
///     can be added once <see cref="StoreImage" /> is set. StellarAdmin never deletes images, so
///     images that are removed or never saved stay where they were stored.
/// </remarks>
public sealed class RichTextEditor : FieldEditor, IImageUploadEditor
{
    /// <summary>
    ///     Gets or sets the image types that can be added.
    /// </summary>
    /// <value>
    ///     The media types, such as <c>image/png</c>. The default is JPEG, PNG, WebP, GIF and AVIF.
    /// </value>
    /// <remarks>
    ///     The list must not be empty when images can be added. The server recognizes the default
    ///     formats by their content, not only by the type the browser declares.
    /// </remarks>
    public IReadOnlyList<string> AcceptedTypes { get; set; } =
    ["image/jpeg", "image/png", "image/webp", "image/gif", "image/avif"];

    /// <summary>
    ///     Gets or sets the largest image that can be added.
    /// </summary>
    /// <value>
    ///     The size in bytes. The default is 5 MB.
    /// </value>
    /// <remarks>
    ///     The value must be positive. The upload request's body limit is raised to allow it.
    /// </remarks>
    public long MaxFileSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    ///     Gets or sets the hint text displayed while the editor is empty.
    /// </summary>
    public string? Placeholder { get; set; }

    /// <summary>
    ///     Gets or sets the number of lines of text the editor has room for before it grows.
    /// </summary>
    /// <value>
    ///     The number of lines, or <see langword="null" /> for the default of 8.
    /// </value>
    /// <remarks>
    ///     A value less than 1 is a configuration problem, and the setting keeps its value.
    /// </remarks>
    public int? Rows
    {
        get;
        set
        {
            if (value is < 1)
            {
                ResourceProblemSink.ReportInvalidNumber(nameof(Rows), value.Value, "1 or more");
                return;
            }

            field = value;
        }
    }

    /// <inheritdoc />
    public override string TemplateName => "Editors/RichText";

    internal bool HasStore => StoreImageAsync is not null;

    bool IImageUploadEditor.HasStore => HasStore;

    internal Func<IFormFile, IServiceProvider, CancellationToken, Task<string>>? StoreImageAsync
    {
        get;
        private set;
    }

    /// <summary>
    ///     Sets a delegate that saves an image added to the text, which lets images be added.
    /// </summary>
    /// <param name="store">
    ///     A delegate that saves the file and returns the URL the text shows the image from.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="store" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The file has already passed the size and type checks. The delegate must not return an
    ///     empty value.
    /// </remarks>
    public void StoreImage(Func<IFormFile, IServiceProvider, CancellationToken, Task<string>> store)
    {
        ArgumentNullException.ThrowIfNull(store);

        StoreImageAsync = store;
    }

    async Task<(string Value, string? Url)> IImageUploadEditor.StoreAsync(
        IFormFile file,
        IServiceProvider services,
        CancellationToken cancellationToken
    )
    {
        var url = await StoreImageAsync!(file, services, cancellationToken);

        return (url, url);
    }
}
