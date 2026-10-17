using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a table in a container that scrolls sideways when the table is too wide.
/// </summary>
/// <remarks>
///     Build the table from the header, body, footer, row, head, cell and caption tag helpers.
///     The <c>class</c> attribute applies to the <c>&lt;table&gt;</c> element, and other
///     attributes, such as <c>id</c>, apply to the element around it. Row selection and row
///     details require <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-table")]
public sealed class TableTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value that indicates whether clicking anywhere in a row toggles its
    ///     details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if a click anywhere in the row toggles its details; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Clicks on links, buttons and form controls keep their own behavior. Applies only with
    ///     <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-click")]
    public bool? RowClick { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether row details animate open and closed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if details animate; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Details don't animate when the user prefers reduced motion. Applies only with
    ///     <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-animate")]
    public bool? RowDetailAnimate { get; set; }

    /// <summary>
    ///     Gets or sets the line between an expanded row and its details.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="TableRowDetailDivider.Inset" />.
    /// </value>
    /// <remarks>
    ///     Applies only with <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-divider")]
    public TableRowDetailDivider? RowDetailDivider { get; set; }

    /// <summary>
    ///     Gets or sets how an expanded row and its details are set apart from the rows around them.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the emphasis. The default is
    ///     <see cref="TableRowDetailEmphasis.Band" />.
    /// </value>
    /// <remarks>
    ///     Applies only with <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-emphasis")]
    public TableRowDetailEmphasis? RowDetailEmphasis { get; set; }

    /// <summary>
    ///     Gets or sets how many rows can be expanded at once.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many rows can be expanded. The default
    ///     is <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </value>
    /// <remarks>
    ///     Applies only with <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-expand-mode")]
    public TableRowDetailExpandMode? RowDetailExpandMode { get; set; }

    /// <summary>
    ///     Gets or sets where the content of a row's details starts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the content starts. The default is
    ///     <see cref="TableRowDetailInset.Aligned" />.
    /// </value>
    /// <remarks>
    ///     Applies only with <c>row-details</c>.
    /// </remarks>
    [HtmlAttributeName("row-detail-inset")]
    public TableRowDetailInset? RowDetailInset { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether rows can expand to show details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if rows can expand; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Place a <c>&lt;sa-table-row-detail&gt;</c> after each row that can expand, and a
    ///     <c>&lt;sa-table-row-detail-toggle&gt;</c> in a cell of that row. To load details when
    ///     they expand, add htmx attributes with <c>hx-trigger="rowdetailexpand once"</c> to the
    ///     <c>&lt;sa-table-row-detail&gt;</c>.
    /// </remarks>
    [HtmlAttributeName("row-details")]
    public bool? RowDetails { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether rows can be selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if rows can be selected; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Place a <c>&lt;sa-table-selection-head&gt;</c> in the header row to select or clear
    ///     every row, and a <c>&lt;sa-table-selection-cell&gt;</c> in each body row. Selected rows
    ///     get <c>data-state="selected"</c>, and the selection posts as checkbox form data. In
    ///     script, read the <c>selectedValues</c> property of the <c>sel-table</c> element, or
    ///     listen for its bubbling <c>rowselectionchange</c> event, whose
    ///     <c>event.detail.values</c> holds the selected values.
    /// </remarks>
    [HtmlAttributeName("selection")]
    public bool? Selection { get; set; }

    /// <inheritdoc />
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
                (RowDetailExpandMode ?? TableRowDetailExpandMode.Multiple).GetDataAttributeText(),
                (RowDetailEmphasis ?? TableRowDetailEmphasis.Band).GetDataAttributeText(),
                (RowDetailDivider ?? TableRowDetailDivider.Inset).GetDataAttributeText(),
                (RowDetailInset ?? TableRowDetailInset.Aligned).GetDataAttributeText(),
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
