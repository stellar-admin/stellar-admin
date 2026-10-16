using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     A full-width row at the top or bottom of the data grid's body, for a button that adds
///     an item. It is shown when the grid has no items too, and hidden while a row is in edit
///     mode. A grid rendered with <c>rows-only</c> leaves it out.
/// </summary>
[HtmlTargetElement("sa-data-grid-add-row", ParentTag = "sa-data-grid")]
public class DataGridAddRowTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Where the row is placed.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="DataGridAddRowPosition.Top" />.
    /// </remarks>
    [HtmlAttributeName("position")]
    public DataGridAddRowPosition Position { get; set; } = DataGridAddRowPosition.Top;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is { Collecting: true })
        {
            gridContext.AddRow = new(
                (await output.GetChildContentAsync(useCachedResult: false)).GetContent(),
                Position
            );
        }

        output.SuppressOutput();
    }
}
