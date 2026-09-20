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
    ///     The page title.
    /// </summary>
    /// <remarks>
    ///     Defaults to the resource's plural label when null.
    /// </remarks>
    public string? Title { get; set; }
}
