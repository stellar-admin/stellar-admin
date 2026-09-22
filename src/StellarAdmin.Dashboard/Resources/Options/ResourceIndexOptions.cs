namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures a resource's index page.
/// </summary>
public sealed class ResourceIndexOptions
{
    /// <summary>
    ///     The columns displayed in order.
    /// </summary>
    public IList<DataGridColumnOptions> Columns { get; } = new List<DataGridColumnOptions>();

    /// <summary>
    ///     The create button label.
    /// </summary>
    /// <remarks>
    ///     Uses the global resource label callback when null.
    /// </remarks>
    public string? CreateLabel { get; set; }

    /// <summary>
    ///     The default ordering, or null to use the data source's default.
    /// </summary>
    public ResourceSort? DefaultSort { get; set; }

    /// <summary>
    ///     The delete button label.
    /// </summary>
    public string? DeleteLabel { get; set; }

    /// <summary>
    ///     The edit link label.
    /// </summary>
    public string? EditLabel { get; set; }

    /// <summary>
    ///     The paging settings, or null when paging is disabled.
    /// </summary>
    public ResourcePagingOptions? Paging { get; set; }

    /// <summary>
    ///     The scope settings, or null when scopes are disabled.
    /// </summary>
    public ResourceScopesOptions? Scopes { get; set; }

    /// <summary>
    ///     The search settings, or null when searching is disabled.
    /// </summary>
    public ResourceSearchOptions? Search { get; set; }

    /// <summary>
    ///     The page title.
    /// </summary>
    /// <remarks>
    ///     Uses the global resource label callback when null.
    /// </remarks>
    public string? Title { get; set; }
}
