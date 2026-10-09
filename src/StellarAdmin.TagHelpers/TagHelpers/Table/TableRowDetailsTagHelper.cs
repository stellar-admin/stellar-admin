using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Lets the rows of the table it wraps expand to show details. Place an
///     <c>sa-table-row-detail</c> after each expandable row and an
///     <c>sa-table-row-detail-toggle</c> in a cell of that row. Renders the
///     <c>sel-table-row-details</c> web component, which raises bubbling
///     <c>row-detail-expand</c> and <c>row-detail-collapse</c> events on the detail row, so
///     htmx attributes on an <c>sa-table-row-detail</c> can load its details with
///     <c>hx-trigger="row-detail-expand once"</c>.
/// </summary>
[HtmlTargetElement("sa-table-row-details")]
public class TableRowDetailsTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Whether clicking anywhere in a row toggles its details. Clicks on links, buttons and
    ///     form controls keep their own behavior.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>false</c>.
    /// </remarks>
    [HtmlAttributeName("row-click")]
    public bool? RowClick { get; set; }

    /// <summary>
    ///     How many rows can be expanded at once.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </remarks>
    [HtmlAttributeName("expand-mode")]
    public TableRowDetailExpandMode? ExpandMode { get; set; }

    /// <summary>
    ///     How an expanded row and its details are set apart from the rows around them.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailEmphasis.Band" />.
    /// </remarks>
    [HtmlAttributeName("emphasis")]
    public TableRowDetailEmphasis? Emphasis { get; set; }

    /// <summary>
    ///     Where the content of a row's details starts.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailInset.Aligned" />.
    /// </remarks>
    [HtmlAttributeName("inset")]
    public TableRowDetailInset? Inset { get; set; }

    /// <summary>
    ///     Whether details animate open and closed. Animation is always off when the user
    ///     prefers reduced motion.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("animate")]
    public bool? Animate { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "sel-table-row-details";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "table-row-details");
        output.Attributes.SetAttribute(
            "data-expand-mode",
            (ExpandMode ?? TableRowDetailExpandMode.Multiple).GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "data-emphasis",
            (Emphasis ?? TableRowDetailEmphasis.Band).GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "data-inset",
            (Inset ?? TableRowDetailInset.Aligned).GetDataAttributeText()
        );
        output.Attributes.SetAttribute("data-animate", (Animate ?? true) ? "true" : "false");
        output.Attributes.SetAttribute("data-row-click", (RowClick ?? false) ? "true" : "false");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-table-row-details", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
