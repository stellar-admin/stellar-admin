namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Specifies how a multi-select lookup editor lays out its selected items in the form.
/// </summary>
public enum MultiLookupSheetEditorLayout
{
    /// <summary>
    ///     A row per item showing the media, title and description.
    /// </summary>
    List,

    /// <summary>
    ///     A chip per item, wrapping inside a box the height of a text input.
    /// </summary>
    Chips,

    /// <summary>
    ///     A single row the height of a text input, naming the first items and counting the rest.
    /// </summary>
    /// <remarks>
    ///     A read-only field shows a list instead.
    /// </remarks>
    Summary,
}
