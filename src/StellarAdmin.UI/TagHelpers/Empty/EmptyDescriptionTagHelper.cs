using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description in an empty state's header.
/// </summary>
[HtmlTargetElement("sa-empty-description")]
public class EmptyDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "empty-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-empty-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
