using Microsoft.Extensions.Options;

namespace StellarAdmin.Resources.Options;

// A column that computes its value has no property to name it, sort it or label it, so its title
// labels it and names it in sort links, and only an expression can sort it
internal sealed class ComputedColumnOptionsValidator<TResource>
    : IValidateOptions<ResourceOptions<TResource>>
{
    public ValidateOptionsResult Validate(string? name, ResourceOptions<TResource> options)
    {
        var failures = new List<string>();
        var columns = options.Index.Columns;
        foreach (var column in columns.Where(column => column.FieldName is null))
        {
            if (string.IsNullOrWhiteSpace(column.Title))
            {
                failures.Add(
                    $"The index column '{column.FieldExpression}' computes its value, so it needs a Title."
                );
            }
            else if (column is { Sortable: true, SortExpression: null })
            {
                failures.Add(
                    $"The index column '{column.Title}' computes its value, so it can only be sorted by an expression. Use Sortable(selector)."
                );
            }
            else if (
                column.Sortable
                && columns.Any(other =>
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
                failures.Add(
                    $"The index column '{column.Title}' is sorted as '{column.SortField}', as another sortable column is. Give it another title."
                );
            }
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
