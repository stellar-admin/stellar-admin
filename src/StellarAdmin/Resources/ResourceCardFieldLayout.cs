namespace StellarAdmin.Resources;

/// <summary>
///     Specifies how the fields of a card in a cards layout are arranged.
/// </summary>
public enum ResourceCardFieldLayout
{
    /// <summary>
    ///     One field per row, with the label at the start and the value at the end.
    /// </summary>
    Rows,

    /// <summary>
    ///     Two columns of fields, each with its label above its value.
    /// </summary>
    Grid,
}
