using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores how index rows expand to show their details.
/// </summary>
public sealed class ResourceRowDetailsOptions
{
    /// <summary>
    ///     Gets or sets a value that indicates whether details animate open and closed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if details animate; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Details don't animate when the user prefers reduced motion.
    /// </remarks>
    public bool Animate { get; set; } = true;

    /// <summary>
    ///     Gets or sets the line between an expanded row and its details.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="TableRowDetailDivider.Inset" />.
    /// </value>
    public TableRowDetailDivider Divider { get; set; } = TableRowDetailDivider.Inset;

    /// <summary>
    ///     Gets or sets how an expanded row and its details are set apart from the rows around
    ///     them.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the emphasis. The default is
    ///     <see cref="TableRowDetailEmphasis.Band" />.
    /// </value>
    public TableRowDetailEmphasis Emphasis { get; set; } = TableRowDetailEmphasis.Band;

    /// <summary>
    ///     Gets or sets how many rows can be expanded at once.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many rows can be expanded. The default
    ///     is <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </value>
    public TableRowDetailExpandMode ExpandMode { get; set; } = TableRowDetailExpandMode.Multiple;

    /// <summary>
    ///     Gets or sets where the content of a row's details starts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the content starts. The default is
    ///     <see cref="TableRowDetailInset.Aligned" />.
    /// </value>
    public TableRowDetailInset Inset { get; set; } = TableRowDetailInset.Aligned;

    /// <summary>
    ///     Gets the content displayed in a row's details.
    /// </summary>
    /// <value>
    ///     The panels, in display order. More than one panel is shown as tabs.
    /// </value>
    public IList<ResourceRowDetailPanelOptions> Panels { get; } =
        new List<ResourceRowDetailPanelOptions>();

    /// <summary>
    ///     Gets or sets a value that indicates whether clicking anywhere in a row toggles its
    ///     details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if a click anywhere in the row toggles its details; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Clicks on links, buttons and form controls keep their own behavior.
    /// </remarks>
    public bool RowClick { get; set; }

    /// <summary>
    ///     Gets or sets where the buttons that expand and collapse rows are placed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the buttons are placed. The default
    ///     is <see cref="DataGridRowDetailToggle.Start" />.
    /// </value>
    public DataGridRowDetailToggle Toggle { get; set; } = DataGridRowDetailToggle.Start;
}
