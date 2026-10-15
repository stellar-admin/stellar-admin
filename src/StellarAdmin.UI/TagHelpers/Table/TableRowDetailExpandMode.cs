namespace StellarAdmin.UI;

/// <summary>
///     How many rows of a table with row details can be expanded at once.
/// </summary>
public enum TableRowDetailExpandMode
{
    /// <summary>Any number of rows can be expanded.</summary>
    Multiple,

    /// <summary>Expanding a row collapses the row that was expanded.</summary>
    Single,
}

internal static class TableRowDetailExpandModeExtensions
{
    extension(TableRowDetailExpandMode mode)
    {
        public string GetDataAttributeText() =>
            mode switch
            {
                TableRowDetailExpandMode.Multiple => "multiple",
                TableRowDetailExpandMode.Single => "single",
                _ => string.Empty,
            };
    }
}
