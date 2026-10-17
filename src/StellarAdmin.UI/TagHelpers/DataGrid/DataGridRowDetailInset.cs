namespace StellarAdmin.UI;

/// <summary>
///     Specifies where the content of a data grid row's details starts.
/// </summary>
public enum DataGridRowDetailInset
{
    /// <summary>
    ///     The content starts at the row's first cell.
    /// </summary>
    Bleed,

    /// <summary>
    ///     The content starts at the column after the toggle's column.
    /// </summary>
    Aligned,
}

internal static class DataGridRowDetailInsetExtensions
{
    extension(DataGridRowDetailInset inset)
    {
        public string GetDataAttributeText() =>
            inset switch
            {
                DataGridRowDetailInset.Bleed => "bleed",
                DataGridRowDetailInset.Aligned => "aligned",
                _ => string.Empty,
            };
    }
}
