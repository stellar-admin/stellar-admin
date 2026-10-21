using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Row details that load when a row expands find the row by its key
internal static class RowDetailsCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (options.KeySelector is not null || options.Index.RowDetails is not { } rowDetails)
        {
            return;
        }

        foreach (var panel in rowDetails.Panels.Where(panel => panel.LoadsOnDemand).ToArray())
        {
            rowDetails.MutablePanels.Remove(panel);
            options.Report(
                ConfigurationProblemKind.RowDetailsWithoutKey,
                $"The row detail panel '{panel.Title ?? panel.PartialName}' loads when a row expands, which requires a resource key. Call UseKey on the resource.",
                "The panel is left out.",
                panel.Source
            );
        }

        if (rowDetails.Panels.Count == 0)
        {
            options.Index.RowDetails = null;
        }
    }
}
