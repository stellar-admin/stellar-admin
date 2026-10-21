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
                        ReportLimits(options, form, "rich text", field.FieldName);
                    }

                    break;
                case { IsReadOnly: false, Editor: ImageEditor { HasStore: false } }:
                    FormItems.Remove(formOptions!, field);
                    options.Report(
                        ConfigurationProblemKind.ImageFieldWithoutStore,
                        $"The {form} form's image field '{field.FieldName}' has nowhere to store uploads. Call StoreImage on its ImageEditor.",
                        "The field is left out."
                    );
                    break;
                case { IsReadOnly: false, Editor: ImageEditor image }:
                    if (HasInvalidLimits(image))
                    {
                        var defaults = new ImageEditor();
                        image.MaxFileSize = Positive(image.MaxFileSize, defaults.MaxFileSize);
                        image.AcceptedTypes = NonEmpty(image.AcceptedTypes, defaults.AcceptedTypes);
                        ReportLimits(options, form, "image", field.FieldName);
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
        string fieldName
    ) =>
        options.Report(
            ConfigurationProblemKind.InvalidUploadLimits,
            $"The {form} form's {kind} field '{fieldName}' requires a positive MaxFileSize and at least one accepted type.",
            "The field uses the default limit and accepted types where its own are invalid."
        );
}
