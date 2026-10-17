namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores how a multi-select lookup editor displays its field in the form.
/// </summary>
public sealed class MultiLookupFieldOptions
{
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
    ///     Gets or sets how the selected items are laid out.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the layout. The default is
    ///     <see cref="MultiLookupSheetEditorLayout.Chips" />.
    /// </value>
    public MultiLookupSheetEditorLayout Layout { get; set; } = MultiLookupSheetEditorLayout.Chips;

    /// <summary>
    ///     Gets or sets a value that indicates whether the selected items' media is displayed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the media is displayed; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    public bool ShowMedia { get; set; } = true;
}
