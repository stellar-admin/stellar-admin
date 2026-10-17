using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A responsive data table, rendered as a <c>&lt;table&gt;</c> inside a scrollable
///     container. Compose it with the header, body, footer, row, head, cell, and caption
///     subcomponents. Set <c>selection</c> or <c>row-details</c> to add row selection or
///     expandable row details: the table is then wrapped in the <c>sel-table</c> web
///     component, which receives the table's <c>id</c> and other attributes.
/// </summary>
[HtmlTargetElement("sa-table")]
public class TableTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether clicking anywhere in a row toggles its details. Clicks on links, buttons and
    ///     form controls keep their own behavior. Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("row-click")]
    public bool? RowClick { get; set; }

    /// <summary>
    ///     Whether details animate open and closed. Animation is always off when the user
    ///     prefers reduced motion. Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-animate")]
    public bool? RowDetailAnimate { get; set; }

    /// <summary>
    ///     The line between an expanded row and its details. Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailDivider.Inset" />.
    /// </remarks>
    [HtmlAttributeName("row-detail-divider")]
    public TableRowDetailDivider? RowDetailDivider { get; set; }

    /// <summary>
    ///     How an expanded row and its details are set apart from the rows around them.
    ///     Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailEmphasis.Band" />.
    /// </remarks>
    [HtmlAttributeName("row-detail-emphasis")]
    public TableRowDetailEmphasis? RowDetailEmphasis { get; set; }

    /// <summary>
    ///     How many rows can be expanded at once. Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </remarks>
    [HtmlAttributeName("row-detail-expand-mode")]
    public TableRowDetailExpandMode? RowDetailExpandMode { get; set; }

    /// <summary>
    ///     Where the content of a row's details starts. Applies with <c>row-details</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailInset.Aligned" />.
    /// </remarks>
    [HtmlAttributeName("row-detail-inset")]
    public TableRowDetailInset? RowDetailInset { get; set; }

    /// <summary>
    ///     Lets rows expand to show details. Place an <c>sa-table-row-detail</c> after each
    ///     expandable row and an <c>sa-table-row-detail-toggle</c> in a cell of that row.
    ///     Bubbling <c>rowdetailexpand</c> and <c>rowdetailcollapse</c> events are raised
    ///     on the detail row, so htmx attributes on an <c>sa-table-row-detail</c> can load its
    ///     details with <c>hx-trigger="rowdetailexpand once"</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("row-details")]
    public bool? RowDetails { get; set; }

    /// <summary>
    ///     Adds row selection: an <c>sa-table-selection-head</c> in the header row selects or
    ///     clears every row, and an <c>sa-table-selection-cell</c> in each body row selects that
    ///     row. Selected rows get <c>data-state="selected"</c>, and the selection posts as
    ///     ordinary checkbox form data. Read the current selection from the
    ///     <c>sel-table</c> element's <c>selectedValues</c> property, or listen for its bubbling
    ///     <c>rowselectionchange</c> event (with the values in <c>event.detail.values</c>).
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("selection")]
    public bool? Selection { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveRowDetails = RowDetails ?? false;
        var effectiveSelection = Selection ?? false;
        var userClass = output.GetUserSuppliedClass();

        var tableTagBuilder = new TagBuilder("table");
        tableTagBuilder.Attributes.Add("data-slot", "table");
        tableTagBuilder.Attributes.Add("class", JoinCssClasses("sa-table", userClass));
        tableTagBuilder.InnerHtml.AppendHtml(await output.GetChildContentAsync());

        output.TagMode = TagMode.StartTagAndEndTag;
        if (!effectiveSelection && !effectiveRowDetails)
        {
            output.TagName = "div";
            output.Attributes.SetAttribute("data-slot", "table-container");
            output.Attributes.SetAttribute("class", "sa-table-container");
            output.Content.AppendHtml(tableTagBuilder);
            return;
        }

        var containerTagBuilder = new TagBuilder("div");
        containerTagBuilder.Attributes.Add("data-slot", "table-container");
        containerTagBuilder.Attributes.Add("class", "sa-table-container");
        containerTagBuilder.InnerHtml.AppendHtml(tableTagBuilder);

        var rowDetails = effectiveRowDetails
            ? new TableRowDetailSettings(
                RowClick ?? false,
                RowDetailExpandMode ?? TableRowDetailExpandMode.Multiple,
                RowDetailEmphasis ?? TableRowDetailEmphasis.Band,
                RowDetailDivider ?? TableRowDetailDivider.Inset,
                RowDetailInset ?? TableRowDetailInset.Aligned,
                RowDetailAnimate ?? true
            )
            : null;

        output.TagName = "sel-table";
        output.Attributes.RemoveAll("class");
        foreach (
            var (name, value) in TableElementRendering.GetAttributes(
                effectiveSelection,
                rowDetails,
                false
            )
        )
        {
            output.Attributes.SetAttribute(name, value);
        }

        output.Content.AppendHtml(containerTagBuilder);
    }
}
