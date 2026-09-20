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
    ///     The page title.
    /// </summary>
    /// <remarks>
    ///     Uses the global resource label callback when null.
    /// </remarks>
    public string? Title { get; set; }
}
