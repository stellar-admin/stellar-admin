using Microsoft.Extensions.Options;

namespace StellarAdmin.Resources.Options;

// Runs after post-configuration, so an integration can bind a field without a setter first
internal sealed class FieldBinderOptionsValidator<TResource>
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
            if (field.Binder is null && field.PropertyPath[^1].SetMethod?.IsPublic != true)
            {
                failures.Add(
                    $"The {form} form's field '{field.FieldName}' has no public setter. Add one, or bind the field with a FieldBinder."
                );
            }
        }
    }
}
