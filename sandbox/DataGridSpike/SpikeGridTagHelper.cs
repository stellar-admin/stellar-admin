using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DataGridSpike;

/// <summary>
///     Spike grid: publishes a mutable context, runs a collect pass, then re-executes its own
///     child content once per row (mode="rerun", the proposed architecture) or invokes deferred
///     per-column delegates captured during the collect pass (mode="deferred", expected to fail
///     with "last column wins" due to execution-context pooling).
/// </summary>
[HtmlTargetElement("spike-grid")]
public class SpikeGridTagHelper : StellarAdminTagHelperBase
{
    private static readonly object[] StringItems = ["alpha", "beta", "gamma"];

    private static readonly object[] BookingItems =
    [
        new SpikeBooking("VT-1001", 1250.50m, "Confirmed"),
        new SpikeBooking("VT-1002", 89.99m, "Pending"),
        new SpikeBooking("VT-1003", 432.00m, "Cancelled"),
    ];

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    [HtmlAttributeName("mode")]
    public string? Mode { get; set; }

    [HtmlAttributeName("data")]
    public string? Data { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var items = Data == "bookings" ? BookingItems : StringItems;
        var ctx = new SpikeGridContext();
        SetContext(context, ctx);

        var idDiscriminator = new UniqueIdDiscriminator();
        SetContext(context, idDiscriminator);

        // Collect pass: columns register themselves and suppress output.
        await output.GetChildContentAsync(useCachedResult: false);

        output.TagName = "div";
        output.Attributes.SetAttribute("data-spike", "grid");
        output.Content.AppendHtml(
            $"<p>collected columns: {string.Join(", ", ctx.Columns.Select(c => c.Name))}</p>\n"
        );
        output.Content.AppendHtml(
            $"<div data-header>header: {string.Join(" | ", ctx.Columns.Select(c => c.Header))}</div>\n"
        );

        ctx.Collecting = false;

        for (var i = 0; i < items.Length; i++)
        {
            ctx.Item = items[i];
            ctx.RowIndex = i;
            ctx.Cells.Clear();
            ViewContext.ViewData[SpikeHtmlExtensions.ItemKey] = items[i];
            idDiscriminator.Value = $"r{i}";

            if (Mode == "deferred")
            {
                foreach (var (name, template) in ctx.DeferredTemplates)
                {
                    try
                    {
                        ctx.Cells.Add($"[{name}]{(await template()).Trim()}");
                    }
                    catch (Exception ex)
                    {
                        ctx.Cells.Add($"[{name}]EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                    }
                }
            }
            else
            {
                // Re-execute the grid's own child content for this row; columns emit into ctx.Cells.
                await output.GetChildContentAsync(useCachedResult: false);
            }

            output.Content.AppendHtml(
                $"<div data-row=\"{i}\">row {i} ({items[i]}): {string.Join(" | ", ctx.Cells)}</div>\n"
            );
        }

        ViewContext.ViewData.Remove(SpikeHtmlExtensions.ItemKey);
    }
}
