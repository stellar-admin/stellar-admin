using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

// The column and span attributes shared by sa-grid, sa-details and the items placed in them.
internal static class GridLayout
{
    private const string AutoStartValue = "auto";
    private const string FullSpanValue = "full";

    public static GridTiers ResolveColumns(
        string elementName,
        int? columns,
        int? columnsBase,
        int? columnsSmall,
        int? columnsMedium,
        int? columnsLarge
    )
    {
        if (columns is not null && columnsMedium is not null)
        {
            throw new InvalidOperationException(
                $"The <{elementName}> element can set either the columns or the columns-md "
                    + "attribute, not both."
            );
        }

        ValidateColumns(elementName, "columns", columns);
        ValidateColumns(elementName, "columns-base", columnsBase);
        ValidateColumns(elementName, "columns-sm", columnsSmall);
        ValidateColumns(elementName, "columns-md", columnsMedium);
        ValidateColumns(elementName, "columns-lg", columnsLarge);

        return ResponsiveGrid.ResolveColumns(
            columnsBase,
            columnsSmall,
            columnsMedium ?? columns,
            columnsLarge
        );
    }

    // Parses the span and start attributes, so an invalid value throws wherever the element is,
    // and returns the item's style for the grid, or null outside a grid or when the item spans
    // one column with no start at every width.
    public static string? PlacementStyle(
        GridContext? grid,
        string elementName,
        IGridPlacement placement
    )
    {
        var span = ParseSpan(elementName, "column-span", placement.ColumnSpan);
        var spanSmall = ParseSpan(elementName, "column-span-sm", placement.ColumnSpanSmall);
        var spanMedium = ParseSpan(elementName, "column-span-md", placement.ColumnSpanMedium);
        var spanLarge = ParseSpan(elementName, "column-span-lg", placement.ColumnSpanLarge);
        var start = ParseStart(elementName, "column-start", placement.ColumnStart);
        var startSmall = ParseStart(elementName, "column-start-sm", placement.ColumnStartSmall);
        var startMedium = ParseStart(elementName, "column-start-md", placement.ColumnStartMedium);
        var startLarge = ParseStart(elementName, "column-start-lg", placement.ColumnStartLarge);

        if (grid is null)
        {
            return null;
        }

        var spans = ResponsiveGrid.ResolveSpan(
            grid.Columns,
            span,
            spanSmall,
            spanMedium,
            spanLarge
        );
        var starts = ResponsiveGrid.ResolveStart(
            grid.Columns,
            spans,
            start,
            startSmall,
            startMedium,
            startLarge
        );
        string?[] styles = [ResponsiveGrid.SpanStyle(spans), ResponsiveGrid.StartStyle(starts)];
        var style = string.Join(';', styles.OfType<string>());

        return style.Length > 0 ? style : null;
    }

    // Wraps the output in a grid item, outside anything already around it, such as a field.
    public static void WrapInItem(TagHelperOutput output, string? placementStyle)
    {
        var item = new TagBuilder("div");
        item.Attributes["data-slot"] = "grid-item";
        item.AddCssClass("sa-grid-item");
        if (placementStyle is not null)
        {
            item.Attributes["style"] = placementStyle;
        }

        var preElement = new DefaultTagHelperContent();
        output.PreElement.MoveTo(preElement);
        output.PreElement.AppendHtml(item.RenderStartTag()).AppendHtml(preElement);
        output.PostElement.AppendHtml(item.RenderEndTag());
    }

    private static int? ParseSpan(string elementName, string attributeName, string? value)
    {
        // Razor writes a string attribute bound to a null expression as an empty string
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        if (string.Equals(text, FullSpanValue, StringComparison.OrdinalIgnoreCase))
        {
            return ResponsiveGrid.FullSpan;
        }

        if (
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var span)
            && span is >= 1 and <= ResponsiveGrid.MaxColumns
        )
        {
            return span;
        }

        throw new InvalidOperationException(
            $"The <{elementName}> {attributeName} attribute must be a number from 1 to "
                + $"{ResponsiveGrid.MaxColumns} or {FullSpanValue}, but was '{value}'."
        );
    }

    private static int? ParseStart(string elementName, string attributeName, string? value)
    {
        // Razor writes a string attribute bound to a null expression as an empty string
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        if (string.Equals(text, AutoStartValue, StringComparison.OrdinalIgnoreCase))
        {
            return ResponsiveGrid.AutoStart;
        }

        if (
            int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var start)
            && start is >= 1 and <= ResponsiveGrid.MaxColumns
        )
        {
            return start;
        }

        throw new InvalidOperationException(
            $"The <{elementName}> {attributeName} attribute must be a number from 1 to "
                + $"{ResponsiveGrid.MaxColumns} or {AutoStartValue}, but was '{value}'."
        );
    }

    private static void ValidateColumns(string elementName, string attributeName, int? value)
    {
        if (value is < 1 or > ResponsiveGrid.MaxColumns)
        {
            throw new InvalidOperationException(
                $"The <{elementName}> {attributeName} attribute must be between 1 and "
                    + $"{ResponsiveGrid.MaxColumns}."
            );
        }
    }
}
