using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Runs after post-configuration, so an integration can bind a field without a setter first
internal static class FieldBinderCheck
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
            if (field.Binder is null && field.PropertyPath[^1].SetMethod?.IsPublic != true)
            {
                FormItems.Remove(formOptions!, field);
                options.Report(
                    ConfigurationProblemKind.FieldWithoutSetter,
                    $"The `{field.FieldName}` field isn't on the {form} form because `{Property(field)}` has no public setter.",
                    $"Add a public setter to `{Property(field)}`, or bind the field with a `FieldBinder`.",
                    field.Source
                );
            }
        }
    }

    private static string Property(FormFieldOptions field) =>
        $"{field.PropertyPath[^1].DeclaringType?.Name}.{field.PropertyPath[^1].Name}";
}
