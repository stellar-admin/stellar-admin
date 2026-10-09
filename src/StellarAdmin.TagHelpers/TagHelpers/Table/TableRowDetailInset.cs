namespace StellarAdmin.TagHelpers;

/// <summary>
///     Where the content of a row's details starts.
/// </summary>
public enum TableRowDetailInset
{
    /// <summary>At the table's edge padding.</summary>
    Bleed,

    /// <summary>In line with the first column after a leading toggle column.</summary>
    Aligned,
}

internal static class TableRowDetailInsetExtensions
{
    extension(TableRowDetailInset inset)
    {
        public string GetDataAttributeText() =>
            inset switch
            {
                TableRowDetailInset.Bleed => "bleed",
                TableRowDetailInset.Aligned => "aligned",
                _ => string.Empty,
            };
    }
}
