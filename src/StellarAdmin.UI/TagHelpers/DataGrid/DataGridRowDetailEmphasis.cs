namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an expanded data grid row and its details are set apart from the rows around
///     them.
/// </summary>
[GenerateDataAttributeText]
public enum DataGridRowDetailEmphasis
{
    /// <summary>
    ///     The row and its details are joined, with no other emphasis.
    /// </summary>
    None,

    /// <summary>
    ///     A tinted background behind the row and its details.
    /// </summary>
    Band,

    /// <summary>
    ///     An accent bar along the left edge of the row and its details.
    /// </summary>
    Rail,
}
