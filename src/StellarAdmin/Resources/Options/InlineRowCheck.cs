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
            if (FindProblem(options, "inline edit row", edit) is { } problem)
            {
                options.Edit = null;
                options.Report(
                    problem.Kind,
                    problem.Message,
                    "Rows can't be edited.",
                    edit.InlineRow.Source
                );
            }
        }

        if (options.Create is { InlineRow: not null } create)
        {
            if (
                (
                    FindProblem(options, "inline create row", create) ?? FindMissingRequired(create)
                ) is
                { } problem
            )
            {
                options.Create = null;
                options.Report(
                    problem.Kind,
                    problem.Message,
                    "New rows can't be created.",
                    create.InlineRow.Source
                );
            }
        }
    }

    private static (ConfigurationProblemKind Kind, string Message)? FindProblem<TResource>(
        ResourceOptions<TResource> options,
        string row,
        ResourceFormOptions form
    )
    {
        if (options.KeyPropertyName is null)
        {
            return (
                ConfigurationProblemKind.InlineRowsWithoutKey,
                $"The {row} requires a resource key. Call UseKey on the resource."
            );
        }

        var editedFields = options
            .Index.Columns.Select(column => column.EditsFieldName ?? column.FieldName)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
        var unmatched = form
            .Fields.Where(field => !field.IsReadOnly && !editedFields.Contains(field.FieldName))
            .Select(field => field.FieldName)
            .ToArray();

        return unmatched.Length == 0
            ? null
            : (
                ConfigurationProblemKind.InlineRowFieldWithoutColumn,
                $"The {row}'s fields {string.Join(", ", unmatched.Select(field => $"'{field}'"))} match no index column. Add a column for each, or name it with EditsField on the column that shows it."
            );
    }

    private static (ConfigurationProblemKind Kind, string Message)? FindMissingRequired(
        ResourceCreateOptions create
    )
    {
        var fields = create
            .Fields.Where(field => !field.IsReadOnly)
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
                $"The inline create row has no field for the required properties {string.Join(", ", missing.Select(name => $"'{name}'"))} of {create.ModelType.Name}. Add the fields and columns for them."
            );
    }
}
