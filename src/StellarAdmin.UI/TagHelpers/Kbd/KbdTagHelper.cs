using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a keyboard key or keystroke.
/// </summary>
[HtmlTargetElement("sa-kbd")]
public sealed class KbdTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        KbdRenderingHelper.RenderKey(output);

        return Task.CompletedTask;
    }
}
