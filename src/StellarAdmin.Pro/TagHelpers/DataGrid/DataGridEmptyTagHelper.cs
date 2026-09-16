using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Pro.TagHelpers;

/// <summary>
///     Custom content for the data grid's empty state, shown as a single full-width row when
///     the grid has no items. When omitted, the grid renders a default "No records found."
///     message.
/// </summary>
[HtmlTargetElement("sa-data-grid-empty", ParentTag = "sa-data-grid")]
public class DataGridEmptyTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        // Renders content and stores it on the grid context to allow the grid to
        // render it conditionally and in the appropriate place
        var gridContext = GetContext<DataGridContext>(context);
        if (gridContext is { Collecting: true })
        {
            gridContext.EmptyContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            ).GetContent();
        }

        output.SuppressOutput();
    }
}
