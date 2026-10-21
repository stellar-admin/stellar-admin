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
                    $"The {form} form's field '{field.FieldName}' has no public setter. Add one, or bind the field with a FieldBinder.",
                    "The field is left out.",
                    field.Source
                );
            }
        }
    }
}
