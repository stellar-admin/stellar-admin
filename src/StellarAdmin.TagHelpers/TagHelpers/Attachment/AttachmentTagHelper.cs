using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A file or image presented with its media, metadata, upload state, and actions.
/// </summary>
[HtmlTargetElement("sa-attachment")]
public class AttachmentTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<AttachmentOrientation, string> AttachmentOrientationClasses =
        new()
        {
            [AttachmentOrientation.Horizontal] = "sa-attachment-orientation-horizontal",
            [AttachmentOrientation.Vertical] = "sa-attachment-orientation-vertical",
        };

    private static readonly Dictionary<AttachmentSize, string> AttachmentSizeClasses = new()
    {
        [AttachmentSize.Default] = "sa-attachment-size-default",
        [AttachmentSize.Small] = "sa-attachment-size-sm",
        [AttachmentSize.ExtraSmall] = "sa-attachment-size-xs",
    };

    /// <summary>
    ///     How the attachment arranges its media and content.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AttachmentOrientation.Horizontal" />.
    /// </remarks>
    [HtmlAttributeName("orientation")]
    public AttachmentOrientation? Orientation { get; set; }

    /// <summary>
    ///     The size of the attachment, controlling its padding and spacing.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AttachmentSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public AttachmentSize? Size { get; set; }

    /// <summary>
    ///     The point the attachment has reached in its upload lifecycle.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AttachmentState.Done" />.
    /// </remarks>
    [HtmlAttributeName("state")]
    public AttachmentState? State { get; set; }

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
            JoinCssClasses(
                "sa-attachment",
                "group/attachment",
                AttachmentSizeClasses[effectiveSize],
                AttachmentOrientationClasses[effectiveOrientation],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
