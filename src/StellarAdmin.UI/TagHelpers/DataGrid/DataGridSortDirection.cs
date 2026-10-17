namespace StellarAdmin.UI;

/// <summary>
///     The direction of a data grid sort.
/// </summary>
public enum DataGridSortDirection
{
    /// <summary>
    ///     Sorts values from lowest to highest.
    /// </summary>
    Ascending,

    /// <summary>
    ///     Sorts values from highest to lowest.
    /// </summary>
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
