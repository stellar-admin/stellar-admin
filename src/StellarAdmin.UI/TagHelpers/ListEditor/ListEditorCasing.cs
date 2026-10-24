namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-list-editor&gt;</c> changes the case of an item as it is entered.
/// </summary>
public enum ListEditorCasing
{
    /// <summary>
    ///     The item keeps the case it is entered in.
    /// </summary>
    None,

    /// <summary>
    ///     The item is changed to uppercase.
    /// </summary>
    Upper,

    /// <summary>
    ///     The item is changed to lowercase.
    /// </summary>
    Lower,
}

internal static class ListEditorCasingExtensions
{
    extension(ListEditorCasing casing)
    {
        public string? GetDataAttributeText() =>
            casing switch
            {
                ListEditorCasing.Upper => "upper",
                ListEditorCasing.Lower => "lower",
                _ => null,
            };
    }
}
