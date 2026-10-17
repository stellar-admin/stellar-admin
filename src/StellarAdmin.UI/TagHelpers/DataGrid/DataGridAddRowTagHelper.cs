using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a full-width row in a data grid, for a button that adds an item.
/// </summary>
/// <remarks>
///     The row also shows when the grid has no items. It is hidden while a new row, with the key
///     <c>new</c>, is in edit mode. A grid rendered with <c>rows-only</c> leaves it out.
/// </remarks>
[HtmlTargetElement("sa-data-grid-add-row", ParentTag = "sa-data-grid")]
public class DataGridAddRowTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets where the row is placed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies where the row is placed. The default is
    ///     <see cref="DataGridAddRowPosition.Top" />.
    /// </value>
    [HtmlAttributeName("position")]
    public DataGridAddRowPosition? Position { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is { Collecting: true })
        {
            gridContext.AddRow = new(
                (await output.GetChildContentAsync(useCachedResult: false)).GetContent(),
                Position ?? DataGridAddRowPosition.Top
            );
        }

        output.SuppressOutput();
    }
}
