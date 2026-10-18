namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Specifies how an <see cref="IconEditor" /> shows its icons on a form.
/// </summary>
public enum IconEditorLayout
{
    /// <summary>
    ///     Tiles when the editor has a list of 24 or fewer icons, and the picker otherwise.
    /// </summary>
    Auto,

    /// <summary>
    ///     A button that opens a searchable grid of the icons.
    /// </summary>
    Picker,

    /// <summary>
    ///     A tile for each icon. Requires a list of icons.
    /// </summary>
    Inline,
}
