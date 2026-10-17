using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a container that groups related content.
/// </summary>
[HtmlTargetElement("sa-card")]
public class CardTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the size of the card, which sets its padding and spacing.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="CardSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public CardSize? Size { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveSize = Size ?? CardSize.Default;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "card");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-card", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
