using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;

namespace StellarAdmin.Resources.Options;

// Grid column counts, spans and starts run from 1 to 12. A number outside that range is clamped to it.
internal static class GridLayoutCheck
{
    public static void Run<TResource>(ResourceOptions<TResource> options)
    {
        CheckScope(options, "create form", options.Create);
        CheckScope(options, "edit form", options.Edit);
        CheckScope(options, "view page", options.View);
    }

    private static void CheckScope<TResource>(
        ResourceOptions<TResource> options,
        string page,
        IFormScope? scope
    )
    {
        if (scope is null)
        {
            return;
        }

        scope.Columns = Clamp(options, page, "column count", scope.Columns, scope.Source);
        foreach (var item in scope.Items)
        {
            CheckItem(options, page, item);
        }
    }

    private static void CheckItem<TResource>(
        ResourceOptions<TResource> options,
        string page,
        FormItemOptions item
    )
    {
        item.ColumnSpan = Clamp(options, page, "column span", item.ColumnSpan, item.Source);
        item.ColumnStart = Clamp(options, page, "column start", item.ColumnStart, item.Source);
        switch (item)
        {
            case FormDetailsOptions details:
                details.Columns = Clamp(
                    options,
                    page,
                    "column count",
                    details.Columns,
                    details.Source
                );
                foreach (var field in details.Fields)
                {
                    CheckItem(options, page, field);
                }

                break;
            case IFormScope container:
                CheckScope(options, page, container);
                break;
            case FormFieldOptions
            {
                Editor: CheckboxGroupEditor { ColumnDefinitions: { } columns } editor
            }:
                editor.ColumnDefinitions = Clamp(
                    options,
                    page,
                    "column count",
                    columns,
                    item.Source
                );
                break;
            case FormFieldOptions
            {
                Editor: RadioGroupEditor { ColumnDefinitions: { } columns } editor
            }:
                editor.ColumnDefinitions = Clamp(
                    options,
                    page,
                    "column count",
                    columns,
                    item.Source
                );
                break;
        }
    }

    private static FormGridColumnDefinitions Clamp<TResource>(
        ResourceOptions<TResource> options,
        string page,
        string setting,
        FormGridColumnDefinitions value,
        ConfigurationSource? source
    )
    {
        var clamped = new FormGridColumnDefinitions(
            Clamp(value.Base, null),
            Clamp(value.Small, null),
            Clamp(value.Medium, null),
            Clamp(value.Large, null)
        );
        Report(options, page, setting, value != clamped, source);

        return clamped;
    }

    private static FormColumnSpanDefinitions Clamp<TResource>(
        ResourceOptions<TResource> options,
        string page,
        string setting,
        FormColumnSpanDefinitions value,
        ConfigurationSource? source
    )
    {
        var clamped = new FormColumnSpanDefinitions(
            Clamp(value.Base, ResponsiveGrid.FullSpan),
            Clamp(value.Small, ResponsiveGrid.FullSpan),
            Clamp(value.Medium, ResponsiveGrid.FullSpan),
            Clamp(value.Large, ResponsiveGrid.FullSpan)
        );
        Report(options, page, setting, value != clamped, source);

        return clamped;
    }

    private static FormColumnStartDefinitions Clamp<TResource>(
        ResourceOptions<TResource> options,
        string page,
        string setting,
        FormColumnStartDefinitions value,
        ConfigurationSource? source
    )
    {
        var clamped = new FormColumnStartDefinitions(
            Clamp(value.Base, ResponsiveGrid.AutoStart),
            Clamp(value.Small, ResponsiveGrid.AutoStart),
            Clamp(value.Medium, ResponsiveGrid.AutoStart),
            Clamp(value.Large, ResponsiveGrid.AutoStart)
        );
        Report(options, page, setting, value != clamped, source);

        return clamped;
    }

    // The full span and the automatic start are outside the range on purpose
    private static int? Clamp(int? value, int? special) =>
        value is null || value == special
            ? value
            : Math.Clamp(value.Value, 1, ResponsiveGrid.MaxColumns);

    private static void Report<TResource>(
        ResourceOptions<TResource> options,
        string page,
        string setting,
        bool clamped,
        ConfigurationSource? source
    )
    {
        if (clamped)
        {
            options.Report(
                ConfigurationProblemKind.InvalidValue,
                $"A {setting} on the {page} is outside 1 to {ResponsiveGrid.MaxColumns}.",
                $"The {page} uses the nearest number in that range.",
                source
            );
        }
    }
}
