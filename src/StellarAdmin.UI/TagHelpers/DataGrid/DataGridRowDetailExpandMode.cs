namespace StellarAdmin.UI;

/// <summary>
///     Specifies how many data grid rows can show their details at once.
/// </summary>
public enum DataGridRowDetailExpandMode
{
    /// <summary>
    ///     Any number of rows can be expanded.
    /// </summary>
    Multiple,

    /// <summary>
    ///     Expanding a row collapses the row that was expanded.
    /// </summary>
    Single,
}

internal static class DataGridRowDetailExpandModeExtensions
{
    extension(DataGridRowDetailExpandMode mode)
    {
        public string GetDataAttributeText() =>
            mode switch
            {
                DataGridRowDetailExpandMode.Multiple => "multiple",
                DataGridRowDetailExpandMode.Single => "single",
                _ => string.Empty,
            };
    }
}
