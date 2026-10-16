using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Editors;

namespace StellarAdmin.Resources.Options;

internal sealed class ImageEditorOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        var failures = new List<string>();
        AddFailures(failures, "create", options.Create);
        AddFailures(failures, "edit", options.Edit);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddFailures(
        List<string> failures,
        string form,
        ResourceFormOptions? formOptions
    )
    {
        foreach (var field in formOptions?.Fields ?? [])
        {
            if (field is not { IsReadOnly: false, Editor: ImageEditor editor })
            {
                continue;
            }

            if (!editor.HasStore)
            {
                failures.Add(
                    $"The {form} form's image field '{field.FieldName}' has nowhere to store uploads. Call StoreImage on its ImageEditor."
                );
            }

            if (editor.MaxFileSize <= 0)
            {
                failures.Add(
                    $"The {form} form's image field '{field.FieldName}' requires a positive MaxFileSize."
                );
            }

            if (editor.AcceptedTypes is not { Count: > 0 })
            {
                failures.Add(
                    $"The {form} form's image field '{field.FieldName}' requires at least one accepted type."
                );
            }
        }
    }
}
