namespace StellarAdmin.Resources;

/// <summary>
///     Provides the details of a dictionary editor field that its text can use.
/// </summary>
/// <param name="fieldLabel">The field's label.</param>
public sealed class DictionaryEditorLabelContext(string fieldLabel)
{
    /// <summary>
    ///     Gets the label of the key or value column the text is about.
    /// </summary>
    /// <value>
    ///     The column's label, or <see langword="null" /> when the text isn't about one column.
    /// </value>
    public string? ColumnLabel { get; init; }

    /// <summary>
    ///     Gets the field's label.
    /// </summary>
    public string FieldLabel { get; } = fieldLabel;

    /// <summary>
    ///     Gets the largest number the column's numbers can be, or <see langword="null" /> for
    ///     none.
    /// </summary>
    public double? Max { get; init; }

    /// <summary>
    ///     Gets the largest number of entries, or <see langword="null" /> for no limit.
    /// </summary>
    public int? MaxItems { get; init; }

    /// <summary>
    ///     Gets the largest number of characters the column's text can have, or
    ///     <see langword="null" /> for no limit.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    ///     Gets the smallest number the column's numbers can be, or <see langword="null" /> for
    ///     none.
    /// </summary>
    public double? Min { get; init; }

    /// <summary>
    ///     Gets the smallest number of entries, or <see langword="null" /> for none.
    /// </summary>
    public int? MinItems { get; init; }
}
