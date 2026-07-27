using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A flexible container that groups related content, composed of a header, title,
///     description, content, footer, and action subcomponents.
/// </summary>
[HtmlTargetElement("sa-card")]
public class CardTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The size of the card, which controls its padding and spacing.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="CardSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public CardSize? Size { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? CardSize.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card", "group/card", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
