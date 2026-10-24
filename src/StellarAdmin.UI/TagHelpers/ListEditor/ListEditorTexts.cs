namespace StellarAdmin.UI;

/// <summary>
///     Specifies the texts a list editor shows as its items change, such as its errors.
/// </summary>
/// <remarks>
///     A text left <see langword="null" /> keeps its default. <c>{n}</c> in a text is replaced by
///     an item's number, <c>{count}</c> by the number of items and <c>{value}</c> by an item's
///     value.
/// </remarks>
public sealed class ListEditorTexts
{
    /// <summary>
    ///     Gets or sets the count shown on the add row when the list has a maximum.
    /// </summary>
    /// <value>
    ///     The default is "{count} of" followed by the maximum, such as "{count} of 8".
    /// </value>
    public string? Count { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when an item is already in the list.
    /// </summary>
    /// <value>
    ///     The default is "{value} is already in the list.".
    /// </value>
    public string? Duplicate { get; set; }

    /// <summary>
    ///     Gets or sets the text shown on the add row when the list is full.
    /// </summary>
    /// <value>
    ///     The default is "You can add up to" followed by the maximum and "items.".
    /// </value>
    public string? Full { get; set; }

    /// <summary>
    ///     Gets or sets the accessible name of an item's input.
    /// </summary>
    /// <value>
    ///     The default is "Item {n}".
    /// </value>
    public string? Item { get; set; }

    /// <summary>
    ///     Gets or sets the accessible name of an item's drag handle.
    /// </summary>
    /// <value>
    ///     The default is "Move item {n}".
    /// </value>
    public string? Move { get; set; }

    /// <summary>
    ///     Gets or sets the text a read-only list shows when it has no items.
    /// </summary>
    /// <value>
    ///     The default is "None".
    /// </value>
    public string? None { get; set; }

    /// <summary>
    ///     Gets or sets the accessible name of an item's remove button.
    /// </summary>
    /// <value>
    ///     The default is "Remove item {n}".
    /// </value>
    public string? Remove { get; set; }

    /// <summary>
    ///     Gets or sets the error shown when the list has fewer items than its minimum.
    /// </summary>
    /// <value>
    ///     The default is "Add at least" followed by the minimum and "items.".
    /// </value>
    public string? TooFew { get; set; }
}
