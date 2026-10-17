namespace StellarAdmin.Resources;

/// <summary>
///     Provides the details of a lookup field that its text can use.
/// </summary>
/// <param name="fieldLabel">The field's label.</param>
/// <param name="minimumSearchLength">
///     The number of characters a search needs before results are shown.
/// </param>
/// <param name="term">
///     The current search term, or <see langword="null" /> when nothing is searched.
/// </param>
public sealed class LookupLabelContext(string fieldLabel, int minimumSearchLength, string? term)
{
    /// <summary>
    ///     Gets the number of items the text counts, such as the selected items a summary doesn't
    ///     name.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    ///     Gets the field's label.
    /// </summary>
    public string FieldLabel { get; } = fieldLabel;

    /// <summary>
    ///     Gets the number of characters a search needs before results are shown.
    /// </summary>
    public int MinimumSearchLength { get; } = minimumSearchLength;

    /// <summary>
    ///     Gets the current search term, or <see langword="null" /> when nothing is searched.
    /// </summary>
    public string? Term { get; } = term;
}
