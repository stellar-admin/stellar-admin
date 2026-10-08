namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     How a lookup editor lays out its selected item in the form.
/// </summary>
public enum LookupSheetEditorLayout
{
    /// <summary>
    ///     A single row the height of a text input, showing the media and title.
    /// </summary>
    Input,

    /// <summary>
    ///     A taller row showing the media, title and description.
    /// </summary>
    Card,
}
