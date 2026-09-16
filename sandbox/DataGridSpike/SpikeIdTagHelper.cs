using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers;

namespace DataGridSpike;

/// <summary>
///     Demonstrates the unique-id problem and the proposed fix: emits both the raw
///     context.UniqueId (a compile-time literal, identical on every row pass) and a
///     discriminated id that appends the ambient row index.
/// </summary>
[HtmlTargetElement("spike-id")]
public class SpikeIdTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var ctx = GetContext<SpikeGridContext>(context);

        output.TagName = "span";
        output.Content.SetContent(
            $"raw-id=spike-{context.UniqueId} discriminated-id=spike-{context.UniqueId}-r{ctx?.RowIndex}"
        );

        return Task.CompletedTask;
    }
}
