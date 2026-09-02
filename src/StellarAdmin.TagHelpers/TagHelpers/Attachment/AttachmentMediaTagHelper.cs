using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The media region of an attachment, holding a file-type icon or a thumbnail preview.
/// </summary>
[HtmlTargetElement("sa-attachment-media")]
public class AttachmentMediaTagHelper : StellarAdminTagHelperBase
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
    ///     The kind of media the region contains, which controls its sizing and styling.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="AttachmentMediaVariant.Icon" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public AttachmentMediaVariant? Variant { get; set; }

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
