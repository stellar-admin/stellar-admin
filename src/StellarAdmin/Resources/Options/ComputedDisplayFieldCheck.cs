using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A display field that computes its value has no property to label it, so its title labels it
internal static class ComputedDisplayFieldCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckFields(options, "view page", options.View);
        CheckFields(options, "create form", options.Create);
        CheckFields(options, "edit form", options.Edit);
    }

    private static void CheckFields<TResource>(
        ResourceOptions<TResource> options,
        string page,
        IFormScope? scope
    )
    {
        foreach (
            var field in FormItems
                .DisplayFields(scope)
                .Where(field => field.FieldName is null && string.IsNullOrWhiteSpace(field.Title))
        )
        {
            FormItems.Remove(scope!, field);
            options.Report(
                ConfigurationProblemKind.ComputedFieldWithoutTitle,
                $"This field isn't on the {page} because it's computed, so it has no property name to use as its title.",
                "Give the field a `Title`.",
                field.Source
            );
        }
    }
}
