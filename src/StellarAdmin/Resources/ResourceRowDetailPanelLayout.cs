using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources;

/// <summary>
///     Represents how a row's details display two or more panels.
/// </summary>
/// <remarks>
///     Panels are shown in an accordion, or as tabs.
/// </remarks>
public abstract class ResourceRowDetailPanelLayout
{
    private protected ResourceRowDetailPanelLayout() { }

    // Why the layout can't be used, when a factory method got an undefined value
    internal ConfigurationValueProblem? Problem { get; init; }

    /// <summary>
    ///     Returns a layout that shows the panels in an accordion, any number of them open at once.
    /// </summary>
    /// <returns>A layout that shows the panels in an accordion.</returns>
    /// <remarks>
    ///     The panels start closed when a row expands.
    /// </remarks>
    public static AccordionResourceRowDetailPanelLayout Accordion() =>
        new(ResourceRowDetailAccordionExpandMode.Multiple);

    /// <summary>
    ///     Returns a layout that shows the panels in an accordion, with the specified number of
    ///     them open at once.
    /// </summary>
    /// <param name="expandMode">How many panels can be open at once.</param>
    /// <returns>A layout that shows the panels in an accordion.</returns>
    /// <remarks>
    ///     The panels start closed when a row expands.
    /// </remarks>
    public static AccordionResourceRowDetailPanelLayout Accordion(
        ResourceRowDetailAccordionExpandMode expandMode
    ) =>
        new(expandMode)
        {
            Problem = Enum.IsDefined(expandMode)
                ? null
                : new(
                    $"the layout's expand mode `{(int)expandMode}` isn't a `ResourceRowDetailAccordionExpandMode` value",
                    "Pass a `ResourceRowDetailAccordionExpandMode` value to `ResourceRowDetailPanelLayout.Accordion`."
                ),
        };

    /// <summary>
    ///     Returns a layout that shows the panels as tabs on a muted background.
    /// </summary>
    /// <returns>A layout that shows the panels as tabs.</returns>
    /// <remarks>
    ///     The first tab is selected when a row expands.
    /// </remarks>
    public static TabsResourceRowDetailPanelLayout Tabs() => new(ResourceRowDetailTabStyle.Default);

    /// <summary>
    ///     Returns a layout that shows the panels as tabs in the specified style.
    /// </summary>
    /// <param name="style">The visual style of the tabs.</param>
    /// <returns>A layout that shows the panels as tabs.</returns>
    /// <remarks>
    ///     The first tab is selected when a row expands.
    /// </remarks>
    public static TabsResourceRowDetailPanelLayout Tabs(ResourceRowDetailTabStyle style) =>
        new(style)
        {
            Problem = Enum.IsDefined(style)
                ? null
                : new(
                    $"the layout's tab style `{(int)style}` isn't a `ResourceRowDetailTabStyle` value",
                    "Pass a `ResourceRowDetailTabStyle` value to `ResourceRowDetailPanelLayout.Tabs`."
                ),
        };
}
