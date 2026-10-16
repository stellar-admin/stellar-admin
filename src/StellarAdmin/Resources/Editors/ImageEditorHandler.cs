namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Displays an image field as an image upload and resolves the current image's URL.
/// </summary>
public sealed class ImageEditorHandler(ImageEditor editor, IServiceProvider services)
    : FieldEditorHandler<ImageEditor>(editor)
{
    /// <inheritdoc />
    public override string TemplateName => "Editors/Image";

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
