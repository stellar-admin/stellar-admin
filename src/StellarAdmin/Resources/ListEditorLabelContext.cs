namespace StellarAdmin.Resources;

/// <summary>
///     Provides the details of a list editor field that its text can use.
/// </summary>
/// <param name="fieldLabel">The field's label.</param>
public sealed class ListEditorLabelContext(string fieldLabel)
{
    /// <summary>
    ///     Gets the field's label.
    /// </summary>
    public string FieldLabel { get; } = fieldLabel;

    /// <summary>
    ///     Gets the largest number a number item can be, or <see langword="null" /> for none.
    /// </summary>
    public double? Max { get; init; }

    /// <summary>
    ///     Gets the largest number of items, or <see langword="null" /> for no limit.
    /// </summary>
    public int? MaxItems { get; init; }

    /// <summary>
    ///     Gets the largest number of characters a text item can have, or
    ///     <see langword="null" /> for no limit.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    ///     Gets the smallest number a number item can be, or <see langword="null" /> for none.
    /// </summary>
    public double? Min { get; init; }

    /// <summary>
    ///     Gets the smallest number of items, or <see langword="null" /> for none.
    /// </summary>
    public int? MinItems { get; init; }
}
