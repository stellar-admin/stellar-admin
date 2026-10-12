using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

// The column and span attributes shared by sa-grid, sa-details and the items placed in them.
internal static class GridLayout
{
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

    // Parses the span attributes, so an invalid value throws wherever the element is, and returns
    // the span style for the grid, or null outside a grid or when the span is 1 at every width.
    public static string? SpanStyle(
        GridContext? grid,
        string elementName,
        string? columnSpan,
        string? columnSpanSmall,
        string? columnSpanMedium,
        string? columnSpanLarge
    )
    {
        var span = ParseSpan(elementName, "column-span", columnSpan);
        var spanSmall = ParseSpan(elementName, "column-span-sm", columnSpanSmall);
        var spanMedium = ParseSpan(elementName, "column-span-md", columnSpanMedium);
        var spanLarge = ParseSpan(elementName, "column-span-lg", columnSpanLarge);

        return grid is null
            ? null
            : ResponsiveGrid.SpanStyle(
                ResponsiveGrid.ResolveSpan(grid.Columns, span, spanSmall, spanMedium, spanLarge)
            );
    }

    // Wraps the output in a grid item, outside anything already around it, such as a field.
    public static void WrapInItem(TagHelperOutput output, string? spanStyle)
    {
        var item = new TagBuilder("div");
        item.Attributes["data-slot"] = "grid-item";
        item.AddCssClass("sa-grid-item");
        if (spanStyle is not null)
        {
            item.Attributes["style"] = spanStyle;
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
