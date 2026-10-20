using Microsoft.Extensions.Options;

namespace StellarAdmin.Resources.Options;

internal sealed class AddAnotherOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        if (options.Create is not { AddAnother: { } addAnother } create)
        {
            return ValidateOptionsResult.Success;
        }

        var fields = create
            .Fields.Where(field => !field.IsReadOnly)
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        var failures = addAnother
            .KeptFields.Where(field => !fields.Contains(field))
            .Select(field =>
                $"KeepForNext names '{field}', which isn't an editable field of the create form. Add the field to the create form, or remove the KeepForNext call."
            )
            .ToArray();

        return failures.Length == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
