namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     How a multi-select lookup editor lays out its selected items in the form.
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
    ///     A single row the height of a text input, naming the first items and counting the rest. A read-only field
    ///     shows a list instead.
    /// </summary>
    Summary,
}
