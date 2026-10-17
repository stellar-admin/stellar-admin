using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Lets the data grid's rows expand to show details. The content is a row template,
///     rendered once per data row with the current item available through
///     <c>Html.GridItem&lt;T&gt;()</c>, below its row and spanning every column. The grid adds a
///     toggle column and wraps its table in the <c>sel-table</c> web component,
///     which raises bubbling <c>rowdetailexpand</c> and <c>rowdetailcollapse</c> events on
///     the detail row, so htmx can load a row's details when it first expands.
/// </summary>
[HtmlTargetElement("sa-data-grid-row-detail", ParentTag = "sa-data-grid")]
public class DataGridRowDetailTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Where the grid places the buttons that expand and collapse rows.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DataGridRowDetailToggle.Start" />.
    /// </remarks>
    [HtmlAttributeName("toggle")]
    public DataGridRowDetailToggle? Toggle { get; set; }

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
    ///     The line between an expanded row and its details.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="TableRowDetailDivider.Inset" />.
    /// </remarks>
    [HtmlAttributeName("divider")]
    public TableRowDetailDivider? Divider { get; set; }

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

    /// <summary>
    ///     The keys of the rows to render expanded, compared against the grid's
    ///     <c>key-field</c>, which is then required. Expanding a row in the browser raises
    ///     <c>rowdetailexpand</c>; a row rendered expanded does not, so render its content
    ///     directly rather than loading it on expand.
    /// </summary>
    [HtmlAttributeName("expanded-keys")]
    public IEnumerable? ExpandedKeys { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is null)
        {
            return;
        }

        if (gridContext.CurrentRow is { } currentRow)
        {
            // An editing row has no details row.
            if (currentRow.Editing)
            {
                return;
            }

            // Row pass: render the template for this row's item.
            currentRow.DetailHtml = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
            return;
        }

        // Collect pass: register the declaration; the template is not executed, so it can
        // dereference the row item unconditionally.
        if (ExpandedKeys is not null && gridContext.KeyField is null)
        {
            throw new InvalidOperationException(
                "<sa-data-grid-row-detail> requires the key-field attribute on the grid when "
                    + "expanded-keys is set."
            );
        }

        gridContext.RowDetail = new DataGridRowDetail
        {
            Toggle = Toggle ?? DataGridRowDetailToggle.Start,
            Settings = new TableRowDetailSettings(
                RowClick ?? false,
                ExpandMode ?? TableRowDetailExpandMode.Multiple,
                Emphasis ?? TableRowDetailEmphasis.Band,
                Divider ?? TableRowDetailDivider.Inset,
                Inset ?? TableRowDetailInset.Aligned,
                Animate ?? true
            ),
            ExpandedKeys = (ExpandedKeys?.Cast<object?>() ?? [])
                .Select(key => Convert.ToString(key, CultureInfo.InvariantCulture))
                .OfType<string>()
                .ToHashSet(),
        };
    }
}
