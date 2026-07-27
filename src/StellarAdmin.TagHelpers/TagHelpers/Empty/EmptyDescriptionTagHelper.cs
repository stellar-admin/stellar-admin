using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A line of muted descriptive text within an empty state header.
/// </summary>
[HtmlTargetElement("sa-empty-description")]
public class EmptyDescriptionTagHelper : StellarAdminTagHelperBase
{
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
