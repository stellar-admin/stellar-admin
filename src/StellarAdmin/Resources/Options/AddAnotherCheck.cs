using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Save and add another keeps the values of create form fields, so it can only keep fields the form edits
internal static class AddAnotherCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (options.Create is not { AddAnother: { } addAnother } create)
        {
            return;
        }

        var fields = create
            .Fields.Where(field => !field.IsReadOnly)
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        foreach (
            var field in addAnother.KeptFields.Where(field => !fields.Contains(field)).ToArray()
        )
        {
            addAnother.KeptFieldNames.Remove(field);
            options.Report(
                ConfigurationProblemKind.UnknownKeptField,
                $"Save and add another doesn't keep `{field}` because it isn't an editable field of the create form.",
                "Add the field to the create form, or remove the `KeepForNext` call.",
                addAnother.KeptFieldSources.GetValueOrDefault(field)
            );
        }
    }
}
