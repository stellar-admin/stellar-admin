using System.ComponentModel.DataAnnotations;
using System.Reflection;
using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// An inline row edits the grid's columns in place and finds its row by the resource key. A row that can't do that
// is removed, with its create or edit, since a row with a missing field could save a partial record.
internal static class InlineRowCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (options.Edit is { InlineRow: not null } edit)
        {
            if (FindProblem(options, edit) is { } problem)
            {
                options.Edit = null;
                options.Report(
                    problem.Kind,
                    $"Rows can't be edited because the inline edit row {problem.Reason}.",
                    problem.Fix,
                    edit.InlineRow.Source
                );
            }
        }

        if (options.Create is { InlineRow: not null } create)
        {
            if (
                (
                    FindProblem(options, create) ?? FindMissingRequired(create)
                ) is
                { } problem
            )
            {
                options.Create = null;
                options.Report(
                    problem.Kind,
                    $"New rows can't be created because the inline create row {problem.Reason}.",
                    problem.Fix,
                    create.InlineRow.Source
                );
            }
        }
    }

    private static (ConfigurationProblemKind Kind, string Reason, string Fix)? FindProblem<TResource>(
        ResourceOptions<TResource> options,
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

        var editedFields = options
            .Index.Columns.Select(column => column.EditsFieldName ?? column.FieldName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        var unmatched = form
            .Fields.Where(field => !field.IsNeverEditable && !editedFields.Contains(field.FieldName))
            .Select(field => field.FieldName)
            .ToArray();

        return unmatched.Length == 0
            ? null
            : (
                ConfigurationProblemKind.InlineRowFieldWithoutColumn,
                $"has fields with no index column: {string.Join(", ", unmatched.Select(field => $"`{field}`"))}",
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
        var missing = create
            .ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property =>
                property.IsDefined(typeof(RequiredAttribute), inherit: true)
                && !fields.Contains(property.Name)
            )
            .Select(property => property.Name)
            .ToArray();

        return missing.Length == 0
            ? null
            : (
                ConfigurationProblemKind.InlineCreateMissingRequired,
                $"has no field for the required properties {string.Join(", ", missing.Select(name => $"`{name}`"))} of `{create.ModelType.Name}`",
                "Add fields and columns for the required properties."
            );
    }
}
