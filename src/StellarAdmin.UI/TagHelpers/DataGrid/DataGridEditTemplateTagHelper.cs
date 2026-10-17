using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the editor of a data grid column in rows that are in edit mode.
/// </summary>
/// <remarks>
///     <para>
///         Use <c>Html.GridItem&lt;T&gt;()</c> to get the row's item. Field names get the prefix
///         <c>rows[key]</c>, so <c>asp-for="@Html.GridItem&lt;Booking&gt;().Destination"</c> posts
///         as <c>rows[24817].Destination</c>.
///     </para>
///     <para>
///         A column without an edit template shows its normal content in an editing row.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-data-grid-edit-template", ParentTag = "sa-data-grid-column")]
public class DataGridEditTemplateTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
