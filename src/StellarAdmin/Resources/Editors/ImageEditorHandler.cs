namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays an image field as an image upload and resolves the image's URL.
/// </summary>
/// <param name="editor">The editor's configuration.</param>
/// <param name="services">The request's service provider.</param>
public sealed class ImageEditorHandler(ImageEditor editor, IServiceProvider services)
    : FieldEditorHandler<ImageEditor>(editor)
{
    /// <inheritdoc />
    public override async Task<object?> PrepareAsync(
        FieldEditorContext context,
        CancellationToken cancellationToken
    ) =>
        new ImageEditorData(
            await Editor.ResolveUrlAsync(context.Value, services, cancellationToken)
        );
}

internal sealed record ImageEditorData(string? Url);
