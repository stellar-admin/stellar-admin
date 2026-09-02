using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A single action control on an attachment, such as removing or retrying it.
/// </summary>
[HtmlTargetElement("sa-attachment-action")]
public class AttachmentActionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the action button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonSize.IconExtraSmall" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     The visual style of the action button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonVariant.Ghost" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "button");
        }

        output.Attributes.SetAttribute("data-slot", "attachment-action");

        ButtonRenderingHelper.RenderAttributes(
            output,
            Variant ?? ButtonVariant.Ghost,
            Size ?? ButtonSize.IconExtraSmall
        );

        return Task.CompletedTask;
    }
}
