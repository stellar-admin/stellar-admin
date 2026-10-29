using StellarAdmin.Diagnostics;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

/// <summary>
///     Stores how index rows expand to show their details.
/// </summary>
public sealed class ResourceRowDetailsOptions
{
    /// <summary>
    ///     Gets a value that indicates whether details animate open and closed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if details animate; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Details don't animate when the user prefers reduced motion.
    /// </remarks>
    public bool Animate { get; internal set; } = true;

    /// <summary>
    ///     Gets the line between an expanded row and its details.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="DataGridRowDetailDivider.Inset" />.
    /// </value>
    public DataGridRowDetailDivider Divider { get; internal set; } = DataGridRowDetailDivider.Inset;

    /// <summary>
    ///     Gets how an expanded row and its details are set apart from the rows around
    ///     them.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the emphasis. The default is
    ///     <see cref="DataGridRowDetailEmphasis.Band" />.
    /// </value>
    public DataGridRowDetailEmphasis Emphasis { get; internal set; } =
        DataGridRowDetailEmphasis.Band;

    /// <summary>
    ///     Gets how many rows can be expanded at once.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many rows can be expanded. The default
    ///     is <see cref="DataGridRowDetailExpandMode.Multiple" />.
    /// </value>
    public DataGridRowDetailExpandMode ExpandMode { get; internal set; } =
        DataGridRowDetailExpandMode.Multiple;

    /// <summary>
    ///     Gets where the content of a row's details starts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the content starts. The default is
    ///     <see cref="DataGridRowDetailInset.Aligned" />.
    /// </value>
    public DataGridRowDetailInset Inset { get; internal set; } = DataGridRowDetailInset.Aligned;

    /// <summary>
    ///     Gets how a row's details display two or more panels.
    /// </summary>
    /// <value>
    ///     The layout. The default is an accordion with any number of panels open at once.
    /// </value>
    public ResourceRowDetailPanelLayout PanelLayout { get; internal set; } =
        ResourceRowDetailPanelLayout.Accordion();

    /// <summary>
    ///     Gets the content displayed in a row's details.
    /// </summary>
    /// <value>
    ///     The panels, in display order. More than one panel is shown as
    ///     <see cref="PanelLayout" /> sets.
    /// </value>
    public IReadOnlyList<ResourceRowDetailPanelOptions> Panels => MutablePanels;

    /// <summary>
    ///     Gets a value that indicates whether clicking anywhere in a row toggles its
    ///     details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if a click anywhere in the row toggles its details; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Clicks on links, buttons and form controls keep their own behavior.
    /// </remarks>
    public bool RowClick { get; internal set; }

    /// <summary>
    ///     Gets where the buttons that expand and collapse rows are placed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the buttons are placed. The default
    ///     is <see cref="DataGridRowDetailToggle.Start" />.
    /// </value>
    public DataGridRowDetailToggle Toggle { get; internal set; } = DataGridRowDetailToggle.Start;

    internal List<ResourceRowDetailPanelOptions> MutablePanels { get; } = [];

    internal ConfigurationSource? PanelLayoutSource { get; set; }
}
