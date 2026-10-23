using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// Row details that load when a row expands need a handler that names their model, and find the row by its key
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
            if (panel.Loader is UnusableHandlerRowDetailLoader loader)
            {
                rowDetails.MutablePanels.Remove(panel);
                ReportHandler(options, panel, loader);
            }
            else if (panel.LoadsOnDemand && options.KeySelector is null)
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
        }
    }

    private static void ReportHandler<TResource>(
        ResourceOptions<TResource> options,
        ResourceRowDetailPanelOptions panel,
        UnusableHandlerRowDetailLoader loader
    )
    {
        var name = panel.Title ?? panel.PartialName;
        var handler = loader.HandlerType.Name;
        if (loader.Models.Count == 0)
        {
            options.Report(
                ConfigurationProblemKind.UnusableRowDetailHandler,
                $"The `{name}` row detail panel isn't shown because `{handler}` doesn't implement `IResourceRowDetailHandler<TModel>`.",
                "Pass a handler that implements `IResourceRowDetailHandler<TModel>` for the partial view's model.",
                panel.Source
            );

            return;
        }

        options.Report(
            ConfigurationProblemKind.UnusableRowDetailHandler,
            $"The `{name}` row detail panel isn't shown because `{handler}` implements `IResourceRowDetailHandler<TModel>` for more than one model ({string.Join(", ", loader.Models.Select(model => $"`{model.Name}`"))}), so the partial view's model isn't known.",
            "Give the partial view's model a handler of its own.",
            panel.Source
        );
    }
}
