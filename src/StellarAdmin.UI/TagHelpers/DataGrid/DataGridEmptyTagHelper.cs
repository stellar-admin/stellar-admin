using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders custom content in a full-width row when the data grid has no items.
/// </summary>
/// <remarks>
///     Without it, the grid shows "No records found.".
/// </remarks>
[HtmlTargetElement("sa-data-grid-empty", ParentTag = "sa-data-grid")]
public sealed class DataGridEmptyTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
