namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-dictionary-editor&gt;</c> aligns a value within its column.
/// </summary>
public enum DictionaryEditorAlign
{
    /// <summary>
    ///     Aligned to the start, as text usually is.
    /// </summary>
    Start,

    /// <summary>
    ///     Aligned to the end, so that the digits of numbers line up.
    /// </summary>
    End,
}

internal static class DictionaryEditorAlignExtensions
{
    extension(DictionaryEditorAlign align)
    {
        public string? GetDataAttributeText() =>
            align switch
            {
                DictionaryEditorAlign.End => "end",
                _ => null,
            };
    }
}
