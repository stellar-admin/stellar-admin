namespace StellarAdmin.Dashboard.Areas.StellarAdmin.ViewModels;

/// <summary>
///     The lookup search settings supplied in the query string.
/// </summary>
public sealed class ResourceLookupQuery
{
    /// <summary>
    ///     The name of the lookup field.
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    ///     The HTML id of the lookup field's editor, which the shared sheet's content belongs to.
    /// </summary>
    public string? For { get; set; }

    /// <summary>
    ///     The form that contains the field: create or edit.
    /// </summary>
    public string? Form { get; set; }

    /// <summary>
    ///     The value of the currently selected item, or null when nothing is selected.
    /// </summary>
    public string? Selected { get; set; }

    /// <summary>
    ///     The number of results to skip.
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    ///     The search term, or null when nothing has been typed.
    /// </summary>
    public string? Term { get; set; }
}
