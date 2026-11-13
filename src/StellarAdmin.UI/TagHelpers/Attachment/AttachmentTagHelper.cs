using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a file with its media, details, upload state and actions.
/// </summary>
[HtmlTargetElement("sa-attachment")]
public sealed class AttachmentTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets how the attachment arranges its media and content.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the arrangement. The default is
    ///     <see cref="AttachmentOrientation.Horizontal" />.
    /// </value>
    [HtmlAttributeName("orientation")]
    public AttachmentOrientation? Orientation { get; set; }

    /// <summary>
    ///     Gets or sets the size of the attachment.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="AttachmentSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public AttachmentSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets how far the attachment has progressed through its upload.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the upload state. The default is
    ///     <see cref="AttachmentState.Done" />.
    /// </value>
    [HtmlAttributeName("state")]
    public AttachmentState? State { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveOrientation = Orientation ?? AttachmentOrientation.Horizontal;
        var effectiveSize = Size ?? AttachmentSize.Default;
        var effectiveState = State ?? AttachmentState.Done;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment");
        output.Attributes.SetAttribute("data-state", effectiveState.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "data-orientation",
            effectiveOrientation.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-attachment", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
