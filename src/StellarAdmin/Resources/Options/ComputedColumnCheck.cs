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
                    ConfigurationProblemEffect.TurnedOff,
                    $"The index column '{column.FieldExpression}' computes its value, so it needs a Title.",
                    "The column is left out."
                );
            }
            else if (column is { Sortable: true, SortExpression: null })
            {
                column.Sortable = false;
                options.Report(
                    ConfigurationProblemKind.ComputedColumnSortedByName,
                    ConfigurationProblemEffect.Ignored,
                    $"The index column '{column.Title}' computes its value, so it can only be sorted by an expression. Use Sortable(selector).",
                    "The column isn't sortable."
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
                    ConfigurationProblemEffect.Ignored,
                    $"The index column '{column.Title}' is sorted as '{column.SortField}', as another sortable column is. Give it another title.",
                    "The column isn't sortable."
                );
            }
        }
    }
}
