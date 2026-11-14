using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of <c>&lt;sa-kbd&gt;</c> keys that form a shortcut or key sequence.
/// </summary>
[HtmlTargetElement("sa-kbd-group")]
public sealed class KbdGroupTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        KbdRenderingHelper.RenderGroup(output);

        return Task.CompletedTask;
    }
}
