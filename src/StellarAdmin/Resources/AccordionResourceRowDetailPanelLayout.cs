namespace StellarAdmin.Resources;

/// <summary>
///     Represents a layout that shows a row's detail panels in an accordion.
/// </summary>
public sealed class AccordionResourceRowDetailPanelLayout : ResourceRowDetailPanelLayout
{
    /// <summary>
    ///     Gets how many panels can be open at once.
    /// </summary>
    public ResourceRowDetailAccordionExpandMode ExpandMode { get; }

    internal AccordionResourceRowDetailPanelLayout(
        ResourceRowDetailAccordionExpandMode expandMode
    ) => ExpandMode = expandMode;
}
