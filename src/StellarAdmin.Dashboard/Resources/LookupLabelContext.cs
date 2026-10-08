namespace StellarAdmin.Dashboard.Resources;

/// <summary>
///     The lookup field details available when generating its text.
/// </summary>
public sealed class LookupLabelContext(string fieldLabel, int minimumSearchLength, string? term)
{
    /// <summary>
    ///     The number of items the text counts, such as the selected items a summary doesn't name.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    ///     The field's label.
    /// </summary>
    public string FieldLabel { get; } = fieldLabel;

    /// <summary>
    ///     The number of characters a search needs before results are shown.
    /// </summary>
    public int MinimumSearchLength { get; } = minimumSearchLength;

    /// <summary>
    ///     The current search term, or <see langword="null" /> when nothing is searched.
    /// </summary>
    public string? Term { get; } = term;
}
