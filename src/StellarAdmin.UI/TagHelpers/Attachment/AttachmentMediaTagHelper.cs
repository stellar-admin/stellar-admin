using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the media of an attachment, such as a file type icon or a thumbnail.
/// </summary>
[HtmlTargetElement("sa-attachment-media")]
public sealed class AttachmentMediaTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<
        AttachmentMediaVariant,
        string
    > AttachmentMediaVariantClasses = new()
    {
        [AttachmentMediaVariant.Icon] = "sa-attachment-media-variant-icon",
        [AttachmentMediaVariant.Image] = "sa-attachment-media-variant-image",
    };

    /// <summary>
    ///     Gets or sets the kind of media shown.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the kind of media. The default is
    ///     <see cref="AttachmentMediaVariant.Icon" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public AttachmentMediaVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? AttachmentMediaVariant.Icon;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "attachment-media");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-attachment-media",
                AttachmentMediaVariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
