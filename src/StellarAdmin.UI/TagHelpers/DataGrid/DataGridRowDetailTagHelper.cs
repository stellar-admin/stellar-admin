using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders details below each data grid row, which users can expand and collapse.
/// </summary>
/// <remarks>
///     <para>
///         The content renders for each row and spans every column. Use
///         <c>Html.GridItem&lt;T&gt;()</c> to get the row's item. Rows in edit mode have no
///         details.
///     </para>
///     <para>
///         The details row raises <c>rowdetailexpand</c> and <c>rowdetailcollapse</c> events, so
///         htmx can load a row's details when it first expands. Requires
///         <c>stellar-admin.js</c>.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-data-grid-row-detail", ParentTag = "sa-data-grid")]
public class DataGridRowDetailTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets where the grid places the buttons that expand and collapse rows.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the buttons go. The default is
    ///     <see cref="DataGridRowDetailToggle.Start" />.
    /// </value>
    [HtmlAttributeName("toggle")]
    public DataGridRowDetailToggle? Toggle { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether clicking anywhere in a row toggles its
    ///     details.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if a click anywhere in a row toggles its details; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Clicks on links, buttons and form controls keep their own behavior.
    /// </remarks>
    [HtmlAttributeName("row-click")]
    public bool? RowClick { get; set; }

    /// <summary>
    ///     Gets or sets how many rows can be expanded at once.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how many rows can be expanded. The default
    ///     is <see cref="TableRowDetailExpandMode.Multiple" />.
    /// </value>
    [HtmlAttributeName("expand-mode")]
    public TableRowDetailExpandMode? ExpandMode { get; set; }

    /// <summary>
    ///     Gets or sets how an expanded row and its details stand out from the rows around them.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the emphasis. The default is
    ///     <see cref="TableRowDetailEmphasis.Band" />.
    /// </value>
    [HtmlAttributeName("emphasis")]
    public TableRowDetailEmphasis? Emphasis { get; set; }

    /// <summary>
    ///     Gets or sets the line between an expanded row and its details.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="TableRowDetailDivider.Inset" />.
    /// </value>
    [HtmlAttributeName("divider")]
    public TableRowDetailDivider? Divider { get; set; }

    /// <summary>
    ///     Gets or sets where the content of a row's details starts.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the content starts. The default is
    ///     <see cref="TableRowDetailInset.Aligned" />.
    /// </value>
    [HtmlAttributeName("inset")]
    public TableRowDetailInset? Inset { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether details animate open and closed.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if details animate; otherwise, <see langword="false" />. The
    ///     default is <see langword="true" />.
    /// </value>
    /// <remarks>
    ///     Animation is always off when the user prefers reduced motion.
    /// </remarks>
    [HtmlAttributeName("animate")]
    public bool? Animate { get; set; }

    /// <summary>
    ///     Gets or sets the keys of the rows to render expanded.
    /// </summary>
    /// <remarks>
    ///     Requires the grid's <c>key-field</c>. A row rendered expanded doesn't raise
    ///     <c>rowdetailexpand</c>, so render its content directly instead of loading it on expand.
    /// </remarks>
    [HtmlAttributeName("expanded-keys")]
    public IEnumerable? ExpandedKeys { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     <see cref="ExpandedKeys" /> is set and the grid has no <c>key-field</c>.
    /// </exception>
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
