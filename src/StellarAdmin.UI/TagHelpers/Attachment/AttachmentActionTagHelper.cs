using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that acts on an attachment, such as removing or retrying it.
/// </summary>
[HtmlTargetElement("sa-attachment-action")]
public sealed class AttachmentActionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ButtonSize.IconExtraSmall" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ButtonSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ButtonVariant.Ghost" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
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
