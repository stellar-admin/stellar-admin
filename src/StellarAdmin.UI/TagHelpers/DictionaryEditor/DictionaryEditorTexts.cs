namespace StellarAdmin.UI;

/// <summary>
///     Specifies the texts a dictionary editor shows as its entries change, such as its errors.
/// </summary>
/// <remarks>
///     A text left <see langword="null" /> keeps its default. <c>{n}</c> in a text is replaced by
///     an entry's number, <c>{count}</c> by the number of entries and <c>{value}</c> by a key.
/// </remarks>
public sealed class DictionaryEditorTexts
{
    /// <summary>
    ///     Gets or sets the count shown on the add row when the dictionary has a maximum.
    /// </summary>
    /// <value>
    ///     The default is "{count} of" followed by the maximum, such as "{count} of 8".
    /// </value>
    public string? Count { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when a key is already used.
    /// </summary>
    /// <value>
    ///     The default is "{value} is already used.".
    /// </value>
    public string? Duplicate { get; set; }

    /// <summary>
    ///     Gets or sets the text shown on the add row when the dictionary is full.
    /// </summary>
    /// <value>
    ///     The default is "You can add up to" followed by the maximum and "items.".
    /// </value>
    public string? Full { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when an entry has a value and no key.
    /// </summary>
    /// <value>
    ///     The default is the key's label followed by "is required.".
    /// </value>
    public string? KeyRequired { get; set; }

    /// <summary>
    ///     Gets or sets the text a read-only dictionary shows when it has no entries.
    /// </summary>
    /// <value>
    ///     The default is "None".
    /// </value>
    public string? None { get; set; }

    /// <summary>
    ///     Gets or sets the accessible name of an entry's remove button.
    /// </summary>
    /// <value>
    ///     The default is "Remove row {n}".
    /// </value>
    public string? Remove { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when the dictionary has fewer entries than its minimum.
    /// </summary>
    /// <value>
    ///     The default is "Add at least" followed by the minimum and "items.".
    /// </value>
    public string? TooFew { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when an entry has a key and no value.
    /// </summary>
    /// <value>
    ///     The default is the value's label followed by "is required.".
    /// </value>
    public string? ValueRequired { get; set; }
}
