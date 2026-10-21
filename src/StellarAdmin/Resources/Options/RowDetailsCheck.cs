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
                $"The `{panel.Title ?? panel.PartialName}` row detail panel isn't shown because it loads when a row expands, which needs a resource key.",
                "Call `UseKey` on the resource.",
                panel.Source
            );
        }

        if (rowDetails.Panels.Count == 0)
        {
            options.Index.RowDetails = null;
        }
    }
}
