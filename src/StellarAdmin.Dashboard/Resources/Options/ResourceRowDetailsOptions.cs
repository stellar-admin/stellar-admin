using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Resources.Options;

/// <summary>
///     Configures the index rows that expand to show details.
/// </summary>
public sealed class ResourceRowDetailsOptions
{
    /// <summary>
    ///     Whether details animate open and closed.
    /// </summary>
    public bool Animate { get; set; } = true;

    /// <summary>
    ///     How an expanded row and its details are set apart from the rows around them.
    /// </summary>
    public TableRowDetailEmphasis Emphasis { get; set; } = TableRowDetailEmphasis.Band;

    /// <summary>
    ///     How many rows can be expanded at once.
    /// </summary>
    public TableRowDetailExpandMode ExpandMode { get; set; } = TableRowDetailExpandMode.Multiple;

    /// <summary>
    ///     Where the content of a row's details starts.
    /// </summary>
    public TableRowDetailInset Inset { get; set; } = TableRowDetailInset.Aligned;

    /// <summary>
    ///     The content displayed in a row's details.
    /// </summary>
    public IList<ResourceRowDetailPanelOptions> Panels { get; } =
        new List<ResourceRowDetailPanelOptions>();

    /// <summary>
    ///     Whether clicking anywhere in a row toggles its details.
    /// </summary>
    public bool RowClick { get; set; }

    /// <summary>
    ///     Where the buttons that expand and collapse rows are placed.
    /// </summary>
    public DataGridRowDetailToggle Toggle { get; set; } = DataGridRowDetailToggle.Leading;
}
