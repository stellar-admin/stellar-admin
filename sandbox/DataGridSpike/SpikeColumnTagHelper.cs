using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DataGridSpike;

/// <summary>
///     Spike column: registers itself during the collect pass; during row passes renders its own
///     child content and pushes the result into the ambient cell collector. With deferred="true"
///     it instead captures a delegate during the collect pass (spike 2's unsound variant).
/// </summary>
[HtmlTargetElement("spike-column")]
public class SpikeColumnTagHelper : StellarAdminTagHelperBase
{
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    [HtmlAttributeName("deferred")]
    public bool? Deferred { get; set; }

    [HtmlAttributeName("field")]
    public string? Field { get; set; }

    [HtmlAttributeName("format")]
    public string? Format { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var ctx = GetContext<SpikeGridContext>(context);
        if (ctx is null)
        {
            output.SuppressOutput();
            return;
        }

        var columnContext = new SpikeColumnContext { Collecting = ctx.Collecting };
        SetContext(context, columnContext);

        var childContent = await output.GetChildContentAsync(useCachedResult: false);

        if (ctx.Collecting)
        {
            ctx.Columns.Add((Name ?? "?", columnContext.HeaderContent ?? Name ?? "?"));

            if (Deferred == true)
            {
                ctx.DeferredTemplates.Add(
                    (
                        Name ?? "?",
                        async () =>
                            (await output.GetChildContentAsync(useCachedResult: false)).GetContent()
                    )
                );
            }

            output.SuppressOutput();
            return;
        }

        // Precedence: item template > inline child content > field binding.
        string cell;
        if (columnContext.ItemContent is not null)
        {
            cell = columnContext.ItemContent;
        }
        else if (!childContent.IsEmptyOrWhiteSpace)
        {
            cell = childContent.GetContent().Trim();
        }
        else if (Field is not null)
        {
            var value = ctx.Item?.GetType().GetProperty(Field)?.GetValue(ctx.Item);
            cell = Format is not null
                ? string.Format(Format, value)
                : value?.ToString() ?? string.Empty;
        }
        else
        {
            cell = string.Empty;
        }

        ctx.Cells.Add($"[{Name}]{cell}");
        output.SuppressOutput();
    }
}
