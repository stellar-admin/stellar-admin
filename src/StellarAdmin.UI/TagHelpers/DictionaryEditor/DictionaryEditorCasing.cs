namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-dictionary-editor&gt;</c> changes the case of a key or value as it is entered.
/// </summary>
public enum DictionaryEditorCasing
{
    /// <summary>
    ///     The text keeps the case it is entered in.
    /// </summary>
    None,

    /// <summary>
    ///     The text is changed to uppercase.
    /// </summary>
    Upper,

    /// <summary>
    ///     The text is changed to lowercase.
    /// </summary>
    Lower,
}

internal static class DictionaryEditorCasingExtensions
{
    extension(DictionaryEditorCasing casing)
    {
        public string? GetDataAttributeText() =>
            casing switch
            {
                DictionaryEditorCasing.Upper => "upper",
                DictionaryEditorCasing.Lower => "lower",
                _ => null,
            };
    }
}
