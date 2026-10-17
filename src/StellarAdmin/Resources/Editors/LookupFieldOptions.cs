namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores how a lookup editor displays its field in the form.
/// </summary>
public sealed class LookupFieldOptions
{
    /// <summary>
    ///     Gets or sets a value that indicates whether the selection can be cleared.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to allow clearing, <see langword="false" /> to prevent it, or
    ///     <see langword="null" /> to allow it when the field is optional. The default is
    ///     <see langword="null" />.
    /// </value>
    public bool? AllowClear { get; set; }

    /// <summary>
    ///     Gets or sets the hint text displayed while nothing is selected.
    /// </summary>
    /// <value>
    ///     The hint text, or <see langword="null" /> to use
    ///     <see cref="StellarAdmin.Resources.Options.LookupLabelOptions.ChooseLabel" />. The
    ///     default is <see langword="null" />.
    /// </value>
    public string? EmptyText { get; set; }

    /// <summary>
    ///     Gets or sets how the selected item is laid out.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout, or <see langword="null" /> to
    ///     use <see cref="LookupSheetEditorLayout.Card" /> when the items have a description and
    ///     <see cref="LookupSheetEditorLayout.Input" /> otherwise. The default is
    ///     <see langword="null" />.
    /// </value>
    public LookupSheetEditorLayout? Layout { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the selected item's media is displayed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the media is displayed; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    public bool ShowMedia { get; set; } = true;
}
