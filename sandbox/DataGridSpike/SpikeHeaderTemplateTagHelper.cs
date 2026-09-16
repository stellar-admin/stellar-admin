using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DataGridSpike;

/// <summary>
///     Header template inside a column: renders its child content only during the collect pass
///     (headers render once) and deposits it on the column context; suppressed on row passes.
/// </summary>
[HtmlTargetElement("spike-header-template", ParentTag = "spike-column")]
public class SpikeHeaderTemplateTagHelper : StellarAdminTagHelperBase
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var columnContext = GetContext<SpikeColumnContext>(context);
        if (columnContext is { Collecting: true })
        {
            columnContext.HeaderContent = (
                await output.GetChildContentAsync(useCachedResult: false)
            )
                .GetContent()
                .Trim();
        }

        output.SuppressOutput();
    }
}
