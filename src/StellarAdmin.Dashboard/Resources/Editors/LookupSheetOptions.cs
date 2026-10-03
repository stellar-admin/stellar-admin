namespace StellarAdmin.Dashboard.Resources.Editors;

/// <summary>
///     Configures the sheet in which a lookup editor searches its items.
/// </summary>
public sealed class LookupSheetOptions
{
    /// <summary>
    ///     The number of characters entered before searching, or 0 to list items when the sheet opens.
    /// </summary>
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
    ///     The number of items loaded at a time.
    /// </summary>
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
    ///     Hint text displayed in the search input.
    /// </summary>
    public string? SearchPlaceholder { get; set; }

    /// <summary>
    ///     Whether each result's media is displayed.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see langword="true" />.
    /// </remarks>
    public bool ShowMedia { get; set; } = true;

    /// <summary>
    ///     The sheet title, or null to use the field label.
    /// </summary>
    public string? Title { get; set; }
}
