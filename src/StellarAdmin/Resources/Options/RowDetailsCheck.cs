using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Row details that load when a row expands find the row by its key; a panel layout made with an undefined value
// falls back to the default
internal static class RowDetailsCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (options.Index.RowDetails is not { } rowDetails)
        {
            return;
        }

        foreach (var panel in rowDetails.Panels.ToArray())
        {
            if (panel.LoadsOnDemand && options.KeySelector is null)
            {
                rowDetails.MutablePanels.Remove(panel);
                options.Report(
                    ConfigurationProblemKind.RowDetailsWithoutKey,
                    $"The `{panel.Title ?? panel.PartialName}` row detail panel isn't shown because it loads when a row expands, which needs a resource key.",
                    "Call `UseKey` on the resource.",
                    panel.Source
                );
            }
        }

        if (rowDetails.Panels.Count == 0)
        {
            options.Index.RowDetails = null;

            return;
        }

        if (rowDetails.PanelLayout.Problem is { } problem)
        {
            rowDetails.PanelLayout = ResourceRowDetailPanelLayout.Accordion();
            options.Report(
                ConfigurationProblemKind.InvalidValue,
                $"The row detail panels are shown in an accordion because {problem.Cause}.",
                problem.Fix,
                rowDetails.PanelLayoutSource
            );
        }
    }
}
