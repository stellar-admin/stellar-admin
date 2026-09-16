using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     The row template of a data grid column, re-rendered once per data row with the current
///     item available through <c>Html.GridItem&lt;T&gt;()</c>. Unlike plain child content, the
///     template is never executed during the grid's collect pass, so it can safely dereference
///     the item unconditionally.
/// </summary>
[HtmlTargetElement("sa-data-grid-item-template", ParentTag = "sa-data-grid-column")]
public class DataGridItemTemplateTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<DataGridColumnContext>(context);
        if (columnContext is { Collecting: false })
        {
            columnContext.ItemContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
        }

        output.SuppressOutput();
    }
}
