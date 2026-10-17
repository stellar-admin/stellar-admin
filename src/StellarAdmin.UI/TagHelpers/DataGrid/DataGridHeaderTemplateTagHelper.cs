using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders custom header content for a data grid column.
/// </summary>
/// <remarks>
///     Takes precedence over the column's <c>title</c>.
/// </remarks>
[HtmlTargetElement("sa-data-grid-header-template", ParentTag = "sa-data-grid-column")]
public sealed class DataGridHeaderTemplateTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<DataGridColumnContext>(context);
        if (columnContext is { Collecting: true })
        {
            columnContext.HeaderContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
        }

        output.SuppressOutput();
    }
}
