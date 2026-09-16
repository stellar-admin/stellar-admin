using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Infrastructure.Query;

internal static class DataGridSortDirectionExtensions
{
    extension(DataGridSortDirection direction)
    {
        public string GetQueryValueText()
        {
            return direction == DataGridSortDirection.Descending ? "desc" : "asc";
        }
    }

    extension(DataGridSortDirection)
    {
        // Anything other than "desc" sorts ascending, so bad input degrades instead of throwing.
        public static DataGridSortDirection ParseQueryValue(string? text)
        {
            return string.Equals(text, "desc", StringComparison.OrdinalIgnoreCase)
                ? DataGridSortDirection.Descending
                : DataGridSortDirection.Ascending;
        }
    }
}
