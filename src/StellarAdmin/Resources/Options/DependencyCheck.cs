using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A field's items can only depend on a field the same form edits, since the form posts the parent's value when it
// changes. A dependency on another field, a repeated one, or one that closes a cycle is dropped.
internal static class DependencyCheck
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
        var fields = FormItems
            .Fields(formOptions)
            .Where(field => !field.IsReadOnly)
            .ToDictionary(field => field.FieldName, StringComparer.Ordinal);
        foreach (var field in FormItems.Fields(formOptions))
        {
            var parents = new HashSet<string>(StringComparer.Ordinal);
            foreach (var dependency in field.Dependencies.ToArray())
            {
                if (!parents.Add(dependency.ParentFieldName))
                {
                    field.Dependencies.Remove(dependency);
                }
                else if (fields.TryGetValue(dependency.ParentFieldName, out var parent))
                {
                    dependency.Parent = parent;
                }
                else
                {
                    field.Dependencies.Remove(dependency);
                    options.Report(
                        ConfigurationProblemKind.UnknownParentField,
                        $"The `{field.FieldName}` field on the {form} form doesn't depend on `{dependency.ParentFieldName}` because it isn't an editable field of the form.",
                        $"Add `{dependency.ParentFieldName}` to the {form} form, or remove the `DependsOn` call.",
                        dependency.Source
                    );
                }
            }
        }

        // A dependency that leads back to its own field would refresh the fields in a loop
        foreach (var field in fields.Values)
        {
            foreach (var dependency in field.Dependencies.ToArray())
            {
                if (!LeadsTo(fields, dependency.ParentFieldName, field.FieldName, []))
                {
                    continue;
                }

                field.Dependencies.Remove(dependency);
                options.Report(
                    ConfigurationProblemKind.DependencyCycle,
                    $"The `{field.FieldName}` field on the {form} form doesn't depend on `{dependency.ParentFieldName}` because `{dependency.ParentFieldName}` already depends on `{field.FieldName}`.",
                    "Remove one of the `DependsOn` calls, so the fields don't depend on each other.",
                    dependency.Source
                );
            }
        }
    }

    // Whether a field depends on the target, directly or through its parents
    private static bool LeadsTo(
        Dictionary<string, FormFieldOptions> fields,
        string fieldName,
        string target,
        HashSet<string> visited
    )
    {
        if (fieldName == target)
        {
            return true;
        }

        return visited.Add(fieldName)
            && fields.TryGetValue(fieldName, out var field)
            && field.Dependencies.Any(dependency =>
                LeadsTo(fields, dependency.ParentFieldName, target, visited)
            );
    }
}
