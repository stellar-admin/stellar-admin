using Microsoft.Extensions.Options;

namespace StellarAdmin.Resources.Options;

// A display field that computes its value has no property to label it, so its title labels it
internal sealed class ComputedDisplayFieldOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        var failures = new List<string>();
        AddFailures(failures, "view page", options.View?.Fields);
        AddFailures(failures, "create form", options.Create?.DisplayFields);
        AddFailures(failures, "edit form", options.Edit?.DisplayFields);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void AddFailures(
        List<string> failures,
        string page,
        IEnumerable<FormDisplayFieldOptions>? fields
    )
    {
        foreach (
            var field in (fields ?? []).Where(field =>
                field.FieldName is null && string.IsNullOrWhiteSpace(field.Title)
            )
        )
        {
            failures.Add(
                $"The {page}'s field '{field.FieldExpression}' computes its value, so it needs a Title."
            );
        }
    }
}
