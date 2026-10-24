namespace StellarAdmin.UI;

/// <summary>
///     Specifies how a <c>&lt;sa-list-editor&gt;</c> aligns an item within its column.
/// </summary>
public enum ListEditorAlign
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

internal static class ListEditorAlignExtensions
{
    extension(ListEditorAlign align)
    {
        public string? GetDataAttributeText() =>
            align switch
            {
                ListEditorAlign.End => "end",
                _ => null,
            };
    }
}
