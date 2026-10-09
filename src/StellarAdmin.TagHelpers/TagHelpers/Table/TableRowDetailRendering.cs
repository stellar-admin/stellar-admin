using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The row details settings rendered on the <c>sel-table-row-details</c> element.
/// </summary>
internal sealed record TableRowDetailSettings(
    bool RowClick,
    TableRowDetailExpandMode ExpandMode,
    TableRowDetailEmphasis Emphasis,
    TableRowDetailInset Inset,
    bool Animate
);

/// <summary>
///     Markup shared by the table row details tag helpers and the data grid, so a grid's
///     generated row details render exactly like hand-written ones.
/// </summary>
internal static class TableRowDetailRendering
{
    public static IEnumerable<KeyValuePair<string, string>> GetWrapperAttributes(
        TableRowDetailSettings settings
    )
    {
        yield return new("data-slot", "table-row-details");
        yield return new("data-expand-mode", settings.ExpandMode.GetDataAttributeText());
        yield return new("data-emphasis", settings.Emphasis.GetDataAttributeText());
        yield return new("data-inset", settings.Inset.GetDataAttributeText());
        yield return new("data-animate", settings.Animate ? "true" : "false");
        yield return new("data-row-click", settings.RowClick ? "true" : "false");
    }

    public static IEnumerable<KeyValuePair<string, string>> GetRowAttributes(bool expanded)
    {
        yield return new("data-slot", "table-row-detail");
        yield return new("data-state", expanded ? "open" : "closed");
        if (!expanded)
        {
            yield return new("hidden", "hidden");
        }
    }

    public static TagBuilder BuildCell(IHtmlContent content, int? colspan)
    {
        // The reveal wrapper animates grid-template-rows to the content's height, the clip
        // hides the content while it collapses, and the content box stays pinned to the
        // visible width when the table scrolls sideways.
        var contentBuilder = new TagBuilder("div");
        contentBuilder.Attributes.Add("data-slot", "table-row-detail-content");
        contentBuilder.Attributes.Add("class", "sa-table-row-detail-content");
        contentBuilder.InnerHtml.AppendHtml(content);

        var clipBuilder = new TagBuilder("div");
        clipBuilder.Attributes.Add("class", "sa-table-row-detail-clip");
        clipBuilder.InnerHtml.AppendHtml(contentBuilder);

        var revealBuilder = new TagBuilder("div");
        revealBuilder.Attributes.Add("class", "sa-table-row-detail-reveal");
        revealBuilder.InnerHtml.AppendHtml(clipBuilder);

        var cellBuilder = new TagBuilder("td");
        cellBuilder.Attributes.Add("data-slot", "table-row-detail-cell");
        cellBuilder.Attributes.Add("class", "sa-table-row-detail-cell");
        if (colspan is { } span)
        {
            cellBuilder.Attributes.Add("colspan", span.ToString(CultureInfo.InvariantCulture));
        }

        cellBuilder.InnerHtml.AppendHtml(revealBuilder);
        return cellBuilder;
    }
}
