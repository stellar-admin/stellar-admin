using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

// A view or edit page that displays the resource itself shows the resource's summary unless it sets its own. A summary
// that separates its items itself has no use for separators added by hand, so they are dropped.
internal static class SummaryCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        if (options.View is { } view)
        {
            view.Summary ??= view.ModelType == typeof(TResource) ? options.Summary : null;
        }

        if (options.Edit is { } edit)
        {
            edit.Summary ??= edit.ModelType == typeof(TResource) ? options.Summary : null;
        }

        var summaries = new[] { options.Summary, options.View?.Summary, options.Edit?.Summary }
            .OfType<ResourceSummaryOptions>()
            .Distinct();
        foreach (var summary in summaries)
        {
            CheckSeparators(options, summary);
        }
    }

    private static void CheckSeparators<TResource>(
        ResourceOptions<TResource> options,
        ResourceSummaryOptions summary
    )
    {
        if (
            !summary.SeparateItems
            || summary.Items.OfType<SummarySeparatorItem>().FirstOrDefault() is not { } separator
        )
        {
            return;
        }

        summary.Items.RemoveAll(item => item is SummarySeparatorItem);
        options.Report(
            ConfigurationProblemKind.SeparatorWithSeparateItems,
            "The summary ignores its `AddSeparator()` separators because `SeparateItems` already separates every two items.",
            "Remove the `AddSeparator()` calls, or don't set `SeparateItems`.",
            separator.Source ?? summary.SeparateItemsSource
        );
    }
}
