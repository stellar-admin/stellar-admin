using System.ComponentModel.DataAnnotations;
using System.Reflection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// An inline row edits the table's columns in place and finds its row by the resource key. A table whose rows can't do
// that edits or adds no rows, since a row with a missing field could save a partial record; it links to the edit or
// create page instead.
internal static class InlineRowCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        foreach (var table in options.Index.RecordTables)
        {
            if (table.InlineEditing is { } editing)
            {
                var problem = options.Edit is { } edit
                    ? FindProblem(options, table, edit)
                    : MissingForm("edit", "Edit");
                if (problem is not null)
                {
                    table.InlineEditing = null;
                    options.Report(
                        problem.Value.Kind,
                        $"The `{table.Key}` table doesn't edit its rows because the inline edit row {problem.Value.Reason}.",
                        problem.Value.Fix,
                        editing.Source
                    );
                }
            }

            if (table.InlineCreate is { } creating)
            {
                var problem = options.Create is { } create
                    ? FindProblem(options, table, create) ?? FindMissingRequired(create)
                    : MissingForm("create", "Create");
                if (problem is not null)
                {
                    table.InlineCreate = null;
                    options.Report(
                        problem.Value.Kind,
                        $"The `{table.Key}` table doesn't add new rows because the inline create row {problem.Value.Reason}.",
                        problem.Value.Fix,
                        creating.Source
                    );
                }
            }
        }
    }

    private static (ConfigurationProblemKind Kind, string Reason, string Fix) MissingForm(
        string form,
        string method
    ) =>
        (
            ConfigurationProblemKind.InlineRowsWithoutForm,
            $"needs the resource's {form} form for its fields",
            $"Call `{method}` on the resource, with the fields the row edits."
        );

    private static (
        ConfigurationProblemKind Kind,
        string Reason,
        string Fix
    )? FindProblem<TResource>(
        ResourceOptions<TResource> options,
        ResourceTableDataLayoutOptions table,
        ResourceFormOptions form
    )
    {
        if (options.KeyPropertyName is null)
        {
            return (
                ConfigurationProblemKind.InlineRowsWithoutKey,
                "needs a resource key to find its row",
                "Call `UseKey` on the resource."
            );
        }

        var editedFields = table
            .Columns.Select(column => column.EditsFieldName ?? column.FieldName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        var unmatched = form
            .Fields.Where(field =>
                !field.IsNeverEditable && !editedFields.Contains(form.GetResourceFieldName(field))
            )
            .Select(field => field.FieldName)
            .ToArray();

        return unmatched.Length == 0
            ? null
            : (
                ConfigurationProblemKind.InlineRowFieldWithoutColumn,
                $"has fields with no column: {string.Join(", ", unmatched.Select(field => $"`{field}`"))}",
                "Add a column for each field, or name the field with `EditsField` on the column that shows it."
            );
    }

    private static (ConfigurationProblemKind Kind, string Reason, string Fix)? FindMissingRequired(
        ResourceCreateOptions create
    )
    {
        var fields = create
            .Fields.Where(field => !field.IsNeverEditable)
            .Select(field => field.FieldName)
            .ToHashSet(StringComparer.Ordinal);
        // A model that wraps the resource also needs fields for the resource's required properties
        var wrapped = create.ResourcePropertyName is { } name
            ? create.ModelType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
            : null;
        var missing = FindRequired(create.ModelType, "")
            .Concat(wrapped is null ? [] : FindRequired(wrapped.PropertyType, wrapped.Name + "."))
            .Where(path => !fields.Contains(path))
            .ToArray();

        return missing.Length == 0
            ? null
            : (
                ConfigurationProblemKind.InlineCreateMissingRequired,
                $"has no field for the required properties {string.Join(", ", missing.Select(name => $"`{name}`"))} of `{create.ModelType.Name}`",
                "Add fields and columns for the required properties."
            );

        static IEnumerable<string> FindRequired(Type type, string prefix) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.IsDefined(typeof(RequiredAttribute), inherit: true))
                .Select(property => prefix + property.Name);
    }
}
