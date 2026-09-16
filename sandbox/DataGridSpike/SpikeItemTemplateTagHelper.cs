using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DataGridSpike;

/// <summary>
///     Item (row) template inside a column: suppressed during the collect pass without executing
///     its child content (so user markup never runs with a null item), and rendered per row pass,
///     depositing the cell content on the column context.
/// </summary>
[HtmlTargetElement("spike-item-template", ParentTag = "spike-column")]
public class SpikeItemTemplateTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<SpikeColumnContext>(context);
        if (columnContext is { Collecting: false })
        {
            columnContext.ItemContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            )
                .GetContent()
                .Trim();
        }

        output.SuppressOutput();
    }
}
