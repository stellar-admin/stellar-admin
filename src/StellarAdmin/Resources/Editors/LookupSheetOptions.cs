namespace StellarAdmin.Resources.Editors;

/// <summary>
///     Stores the settings of the sheet in which a lookup editor searches its items.
/// </summary>
public sealed class LookupSheetOptions
{
    /// <summary>
    ///     Gets or sets the number of characters the user enters before the sheet searches.
    /// </summary>
    /// <value>
    ///     The number of characters, or 0 to list items when the sheet opens. The default is 0.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value is negative.
    /// </exception>
    public int MinimumSearchLength
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            field = value;
        }
    }

    /// <summary>
    ///     Gets or sets the number of items loaded at a time.
    /// </summary>
    /// <value>
    ///     The number of items. The default is 20.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The value is less than 1.
    /// </exception>
    public int PageSize
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            field = value;
        }
    } = 20;

    /// <summary>
    ///     Gets or sets the hint text displayed in the search input.
    /// </summary>
    public string? SearchPlaceholder { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each result's media is displayed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the media is displayed; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    public bool ShowMedia { get; set; } = true;

    /// <summary>
    ///     Gets or sets the sheet's title.
    /// </summary>
    /// <value>
    ///     The title, or <see langword="null" /> to use the field's label. The default is
    ///     <see langword="null" />.
    /// </value>
    public string? Title { get; set; }
}
