using StellarAdmin.Diagnostics;

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
    /// <remarks>
    ///     A negative value is a configuration problem, and the setting keeps its value.
    /// </remarks>
    public int MinimumSearchLength
    {
        get;
        set
        {
            if (value < 0)
            {
                ResourceProblemSink.ReportInvalidNumber(
                    nameof(MinimumSearchLength),
                    value,
                    "0 or more"
                );
                return;
            }

            field = value;
        }
    }

    /// <summary>
    ///     Gets or sets the number of items loaded at a time.
    /// </summary>
    /// <value>
    ///     The number of items. The default is 20.
    /// </value>
    /// <remarks>
    ///     A value less than 1 is a configuration problem, and the setting keeps its value.
    /// </remarks>
    public int PageSize
    {
        get;
        set
        {
            if (value < 1)
            {
                ResourceProblemSink.ReportInvalidNumber(nameof(PageSize), value, "1 or more");
                return;
            }

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
