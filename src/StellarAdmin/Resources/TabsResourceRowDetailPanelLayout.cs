namespace StellarAdmin.Resources;

/// <summary>
///     Represents a layout that shows a row's detail panels as tabs.
/// </summary>
public sealed class TabsResourceRowDetailPanelLayout : ResourceRowDetailPanelLayout
{
    /// <summary>
    ///     Gets the visual style of the tabs.
    /// </summary>
    public ResourceRowDetailTabStyle Style { get; }

    internal TabsResourceRowDetailPanelLayout(ResourceRowDetailTabStyle style) => Style = style;
}
