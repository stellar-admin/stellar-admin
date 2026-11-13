namespace StellarAdmin.UI;

/// <summary>
///     Specifies how many data grid rows can show their details at once.
/// </summary>
[GenerateDataAttributeText]
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
