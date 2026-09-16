using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     Custom header content for a data grid column, rendered once and taking precedence over
///     the column's <c>title</c> attribute.
/// </summary>
[HtmlTargetElement("sa-data-grid-header-template", ParentTag = "sa-data-grid-column")]
public class DataGridHeaderTemplateTagHelper : StellarAdminTagHelperBase
{
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
