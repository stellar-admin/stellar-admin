using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

// An image field needs somewhere to store uploads, and upload limits that accept a file
internal static class ImageEditorCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckFields(options, "create", options.Create);
        CheckFields(options, "edit", options.Edit);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string form,
        ResourceFormOptions? formOptions
    )
    {
        foreach (var field in FormItems.Fields(formOptions))
        {
            switch (field)
            {
                case { IsReadOnly: false, Editor: RichTextEditor { HasStore: true } richText }:
                    if (HasInvalidLimits(richText))
                    {
                        var defaults = new RichTextEditor();
                        richText.MaxFileSize = Positive(richText.MaxFileSize, defaults.MaxFileSize);
                        richText.AcceptedTypes = NonEmpty(
                            richText.AcceptedTypes,
                            defaults.AcceptedTypes
                        );
                        ReportLimits(options, form, "rich text", field);
                    }

                    break;
                case { IsReadOnly: false, Editor: ImageEditor { HasStore: false } }:
                    FormItems.Remove(formOptions!, field);
                    options.Report(
                        ConfigurationProblemKind.ImageFieldWithoutStore,
                        $"The `{field.FieldName}` field isn't on the {form} form because its image editor has nowhere to store uploads.",
                        "Call `StoreImage` on its `ImageEditor`.",
                        field.Source
                    );
                    break;
                case { IsReadOnly: false, Editor: ImageEditor image }:
                    if (HasInvalidLimits(image))
                    {
                        var defaults = new ImageEditor();
                        image.MaxFileSize = Positive(image.MaxFileSize, defaults.MaxFileSize);
                        image.AcceptedTypes = NonEmpty(image.AcceptedTypes, defaults.AcceptedTypes);
                        ReportLimits(options, form, "image", field);
                    }

                    break;
            }
        }
    }

    private static bool HasInvalidLimits(IImageUploadEditor editor) =>
        editor.MaxFileSize <= 0 || editor.AcceptedTypes is not { Count: > 0 };

    private static IReadOnlyList<string> NonEmpty(
        IReadOnlyList<string>? types,
        IReadOnlyList<string> defaults
    ) => types is { Count: > 0 } ? types : defaults;

    private static long Positive(long size, long defaultSize) => size > 0 ? size : defaultSize;

    private static void ReportLimits<TResource>(
        ResourceOptions<TResource> options,
        string form,
        string kind,
        FormFieldOptions field
    ) =>
        options.Report(
            ConfigurationProblemKind.InvalidUploadLimits,
            $"The `{field.FieldName}` {kind} field on the {form} form uses the default `MaxFileSize` or `AcceptedTypes` because its own aren't valid.",
            "Set `MaxFileSize` to a number greater than 0, and list at least one type in `AcceptedTypes`.",
            field.Source
        );
}
