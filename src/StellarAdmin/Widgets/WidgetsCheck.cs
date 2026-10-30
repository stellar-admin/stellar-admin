using StellarAdmin.Diagnostics;

namespace StellarAdmin.Widgets;

// Leaves out the widgets that can't be shown, and resets settings a widget can't use, with a problem for each
internal static class WidgetsCheck
{
    private static readonly TimeSpan ShortestRefreshInterval = TimeSpan.FromSeconds(1);

    public static List<ConfigurationProblem> Run(List<WidgetOptions> widgets, string page)
    {
        var problems = new List<ConfigurationProblem>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var widget in widgets.ToArray())
        {
            if (FindProblem(widget, names) is { } problem)
            {
                widgets.Remove(widget);
                problems.Add(problem with { Resource = page, Source = widget.Source });

                continue;
            }

            problems.AddRange(
                ResetSettings(widget)
                    .Select(setting => setting with { Resource = page, Source = widget.Source })
            );
        }

        return problems;
    }

    private static ConfigurationProblem? FindProblem(WidgetOptions widget, HashSet<string> names)
    {
        if (string.IsNullOrWhiteSpace(widget.Name))
        {
            return new(
                ConfigurationProblemKind.BlankWidgetName,
                $"A widget that shows `{widget.PartialName}` isn't shown because its name is empty.",
                "Give the widget a name that no other widget on the page has."
            );
        }

        if (!names.Add(widget.Name))
        {
            return new(
                ConfigurationProblemKind.RepeatedWidgetName,
                $"The second `{widget.Name}` widget isn't shown because another widget on the page has its name.",
                "Give each widget on the page its own name."
            );
        }

        if (string.IsNullOrWhiteSpace(widget.PartialName))
        {
            return new(
                ConfigurationProblemKind.BlankWidgetPartial,
                $"The `{widget.Name}` widget isn't shown because its partial view name is empty.",
                "Pass the name of the partial view that renders the widget."
            );
        }

        var handler = widget.Loader?.HandlerType.Name;
        if (widget.Loader is UnusableWidgetLoader { Interfaces.Count: 0 })
        {
            return new(
                ConfigurationProblemKind.UnusableWidgetHandler,
                $"The `{widget.Name}` widget isn't shown because `{handler}` doesn't implement `IWidgetHandler<TModel>` or `IWidgetHandler<TParameters, TModel>`.",
                "Pass a handler that implements one of them for the partial view's model."
            );
        }

        if (widget.Loader is UnusableWidgetLoader)
        {
            return new(
                ConfigurationProblemKind.UnusableWidgetHandler,
                $"The `{widget.Name}` widget isn't shown because `{handler}` implements more than one widget handler interface, so the partial view's model isn't known.",
                "Give each widget a handler of its own."
            );
        }

        if (widget.Loader is not { } loader)
        {
            return null;
        }

        if (loader.ParametersType is null && widget.ParametersType is { } unused)
        {
            return new(
                ConfigurationProblemKind.MismatchedWidgetParameters,
                $"The `{widget.Name}` widget isn't shown because it has `{unused.Name}` parameters, but `{handler}` implements `IWidgetHandler<TModel>`, which takes none.",
                $"Implement `IWidgetHandler<{unused.Name}, TModel>` on `{handler}`, or remove the widget's `Parameters` call."
            );
        }

        if (loader.ParametersType is { } expected && widget.ParametersType is null)
        {
            return new(
                ConfigurationProblemKind.MismatchedWidgetParameters,
                $"The `{widget.Name}` widget isn't shown because `{handler}` takes `{expected.Name}` parameters, and the widget doesn't set them.",
                $"Call `Parameters` on the widget with a delegate that returns `{expected.Name}`."
            );
        }

        if (
            loader.ParametersType is { } parameters
            && !parameters.IsAssignableFrom(widget.ParametersType)
        )
        {
            return new(
                ConfigurationProblemKind.MismatchedWidgetParameters,
                $"The `{widget.Name}` widget isn't shown because `{handler}` takes `{parameters.Name}` parameters, but the widget's `Parameters` delegate returns `{widget.ParametersType!.Name}`.",
                $"Return `{parameters.Name}` from the widget's `Parameters` delegate."
            );
        }

        return null;
    }

    private static IEnumerable<ConfigurationProblem> ResetSettings(WidgetOptions widget)
    {
        if (widget.Span is < 1 or > WidgetOptions.DefaultSpan)
        {
            yield return new(
                ConfigurationProblemKind.InvalidValue,
                $"The `{widget.Name}` widget spans 12 columns because `Span` is `{widget.Span}`, which isn't from 1 to 12.",
                "Set `Span` to a number from 1 to 12."
            );

            widget.Span = WidgetOptions.DefaultSpan;
        }

        if (widget.Loader is null && widget.ParametersType is not null)
        {
            yield return new(
                ConfigurationProblemKind.IgnoredWidgetSetting,
                $"The `{widget.Name}` widget's parameters aren't used because the widget has no handler; its partial view gets the page's context.",
                "Add the widget with a handler that takes the parameters, or remove the `Parameters` call."
            );

            widget.Parameters = null;
            widget.ParametersType = null;
        }

        if (widget.Loader is null && widget.RefreshInterval is not null)
        {
            yield return new(
                ConfigurationProblemKind.IgnoredWidgetSetting,
                $"The `{widget.Name}` widget doesn't refresh because it has no handler, so it renders only with the page.",
                "Add the widget with a handler, or remove `RefreshInterval`."
            );

            widget.RefreshInterval = null;
        }
        else if (widget.RefreshInterval < ShortestRefreshInterval)
        {
            yield return new(
                ConfigurationProblemKind.InvalidValue,
                $"The `{widget.Name}` widget doesn't refresh because `RefreshInterval` is shorter than one second.",
                "Set `RefreshInterval` to one second or longer, or remove it."
            );

            widget.RefreshInterval = null;
        }
    }
}
