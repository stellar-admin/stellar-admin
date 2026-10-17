using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an icon or illustration above an empty state's title.
/// </summary>
[HtmlTargetElement("sa-empty-media")]
public sealed class EmptyMediaTagHelper : StellarAdminTagHelperBase
{
    private static readonly Dictionary<EmptyMediaVariant, string> VariantClasses = new Dictionary<
        EmptyMediaVariant,
        string
    >
    {
        [EmptyMediaVariant.Default] = "sa-empty-media-default",
        [EmptyMediaVariant.Icon] = "sa-empty-media-icon",
    };

    /// <summary>
    ///     Gets or sets the visual style of the media.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the visual style. The default is
    ///     <see cref="EmptyMediaVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public EmptyMediaVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? EmptyMediaVariant.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-icon");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-empty-media",
                VariantClasses[effectiveVariant],
                output.GetUserSuppliedClass()
            )
        );

        return Task.CompletedTask;
    }
}
