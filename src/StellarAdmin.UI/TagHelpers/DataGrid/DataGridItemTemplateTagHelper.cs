using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content of a data grid column for each row.
/// </summary>
/// <remarks>
///     Use <c>Html.GridItem&lt;T&gt;()</c> to get the row's item. Unlike the column's plain
///     content, the template only runs for a row, so it can always use the item.
/// </remarks>
[HtmlTargetElement("sa-data-grid-item-template", ParentTag = "sa-data-grid-column")]
public class DataGridItemTemplateTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<DataGridColumnContext>(context);
        if (columnContext is { Collecting: false, Editing: false })
        {
            columnContext.ItemContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
        }

        output.SuppressOutput();
    }
}
