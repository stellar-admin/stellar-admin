using Microsoft.Extensions.Options;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.Resources.Options;

internal sealed class IconEditorOptionsValidator<TResource>(IOptions<IconOptions> iconOptions)
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

    private void AddFailures(List<string> failures, string form, ResourceFormOptions? formOptions)
    {
        foreach (var field in formOptions?.Fields ?? [])
        {
            if (field.Editor is not IconEditor editor)
            {
                continue;
            }

            if (editor.Icons is { Count: 0 })
            {
                failures.Add(
                    $"The {form} form's icon field '{field.FieldName}' has an empty list of icons. Set Icons to null to offer every registered icon."
                );
            }

            if (editor is { Layout: IconEditorLayout.Inline, Icons: null })
            {
                failures.Add(
                    $"The {form} form's icon field '{field.FieldName}' uses the inline layout, which requires a list of icons."
                );
            }

            var unknown = (editor.Icons ?? [])
                .Where(icon => !iconOptions.Value.TryGetIcon(icon, out _))
                .ToArray();
            if (unknown.Length > 0)
            {
                failures.Add(
                    $"The {form} form's icon field '{field.FieldName}' lists icons that aren't registered: {string.Join(", ", unknown)}."
                );
            }
        }
    }
}
