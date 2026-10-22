using System.Collections;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StellarAdmin.Resources.Options;

namespace StellarAdmin.Areas.StellarAdmin;

// A field whose items depend on other fields is disabled while any of them has no value, and its items aren't loaded
// until they all have one
internal static class FieldDependencies
{
    // The fields that depend on a field, directly or through other dependents, in the form's order
    public static FormFieldOptions[] FindDependents(
        IReadOnlyList<FormFieldOptions> fields,
        string fieldName
    )
    {
        var names = new HashSet<string>(StringComparer.Ordinal) { fieldName };
        bool added;
        do
        {
            added = false;
            foreach (var field in fields)
            {
                if (
                    !names.Contains(field.FieldName)
                    && field.Dependencies.Any(dependency =>
                        names.Contains(dependency.ParentFieldName)
                    )
                )
                {
                    names.Add(field.FieldName);
                    added = true;
                }
            }
        } while (added);

        return fields
            .Where(field => field.FieldName != fieldName && names.Contains(field.FieldName))
            .ToArray();
    }

    public static string GetLabel(FormFieldOptions field, IModelMetadataProvider metadataProvider)
    {
        var property = field.PropertyPath[^1];

        return field.Title
            ?? metadataProvider
                .GetMetadataForProperty(property.DeclaringType!, property.Name)
                .GetDisplayName();
    }

    // The text of the disabled field while one of its parents has no value, or null when they all have one
    public static string? GetWaitingText(
        FormFieldOptions field,
        object model,
        IServiceProvider services,
        IModelMetadataProvider metadataProvider
    )
    {
        foreach (var dependency in field.Dependencies)
        {
            if (dependency.Parent is { } parent && IsEmpty(parent.GetValue(model, services)))
            {
                return dependency.EmptyText ?? $"Select {GetLabel(parent, metadataProvider)} first";
            }
        }

        return null;
    }

    // A value that a form can't have chosen: nothing, blank text, an empty collection, or the default of a number, date
    // or ID, which a new record starts with. A Boolean or an enum always has a value.
    public static bool IsEmpty(object? value) =>
        value switch
        {
            null => true,
            string text => string.IsNullOrWhiteSpace(text),
            IEnumerable values => !values.Cast<object?>().Any(),
            bool or Enum => false,
            _ when value.GetType().IsValueType => value.Equals(
                Activator.CreateInstance(value.GetType())
            ),
            _ => false,
        };
}
