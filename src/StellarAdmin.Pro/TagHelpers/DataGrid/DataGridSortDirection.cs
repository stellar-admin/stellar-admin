namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     The direction of a data grid sort.
/// </summary>
public enum DataGridSortDirection
{
    /// <summary>The data is sorted in ascending order.</summary>
    Ascending,

    /// <summary>The data is sorted in descending order.</summary>
    Descending,
}

internal static class DataGridSortDirectionExtensions
{
    extension(DataGridSortDirection direction)
    {
        public string GetQueryValueText() =>
            direction switch
            {
                DataGridSortDirection.Ascending => "asc",
                DataGridSortDirection.Descending => "desc",
                _ => string.Empty,
            };
    }
}
