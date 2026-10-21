using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A column that computes its value has no property to name it, sort it or label it, so its title
// labels it and names it in sort links, and only an expression can sort it
internal static class ComputedColumnCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        var index = options.Index;
        foreach (var column in index.Columns.Where(column => column.FieldName is null).ToArray())
        {
            if (string.IsNullOrWhiteSpace(column.Title))
            {
                index.MutableColumns.Remove(column);
                options.Report(
                    ConfigurationProblemKind.ComputedColumnWithoutTitle,
                    "This column isn't shown because it's computed, so it has no property name to use as its title.",
                    "Give the column a `Title`.",
                    column.Source
                );
            }
            else if (column is { Sortable: true, SortExpression: null })
            {
                column.Sortable = false;
                options.Report(
                    ConfigurationProblemKind.ComputedColumnSortedByName,
                    $"The `{column.Title}` column isn't sortable because it's computed, so it has no property to sort by.",
                    "Pass the value to sort by to `Sortable(selector)`.",
                    column.Source
                );
            }
            else if (
                column.Sortable
                && index.Columns.Any(other =>
                    other != column
                    && other.Sortable
                    && string.Equals(
                        other.SortField,
                        column.SortField,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
            )
            {
                column.Sortable = false;
                options.Report(
                    ConfigurationProblemKind.RepeatedSortName,
                    $"The `{column.Title}` column isn't sortable because another sortable column already sorts as `{column.SortField}`.",
                    "Give the column another title.",
                    column.Source
                );
            }
        }
    }
}
