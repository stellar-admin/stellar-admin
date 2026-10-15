using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     The editor of a data grid column, rendered instead of the column's display in the rows
///     the grid renders in edit mode, with the current item available through
///     <c>Html.GridItem&lt;T&gt;()</c>. Field names get the row's prefix,
///     <c>rows[key]</c>, so <c>asp-for="@Html.GridItem&lt;Booking&gt;().Destination"</c> posts
///     as <c>rows[24817].Destination</c>. A column without an edit template keeps its display
///     in an editing row.
/// </summary>
[HtmlTargetElement("sa-data-grid-edit-template", ParentTag = "sa-data-grid-column")]
public class DataGridEditTemplateTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<DataGridColumnContext>(context);
        if (columnContext is { Collecting: true })
        {
            columnContext.HasEditTemplate = true;
        }
        else if (columnContext is { Editing: true })
        {
            columnContext.EditContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
        }

        output.SuppressOutput();
    }
}
