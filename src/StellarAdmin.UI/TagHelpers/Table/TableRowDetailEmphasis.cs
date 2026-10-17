namespace StellarAdmin.UI;

/// <summary>
///     Specifies how an expanded table row and its details are set apart from the rows around
///     them.
/// </summary>
public enum TableRowDetailEmphasis
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

internal static class TableRowDetailEmphasisExtensions
{
    extension(TableRowDetailEmphasis emphasis)
    {
        public string GetDataAttributeText() =>
            emphasis switch
            {
                TableRowDetailEmphasis.None => "none",
                TableRowDetailEmphasis.Band => "band",
                TableRowDetailEmphasis.Rail => "rail",
                _ => string.Empty,
            };
    }
}
