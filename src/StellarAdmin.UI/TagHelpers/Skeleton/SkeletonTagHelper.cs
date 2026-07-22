using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A placeholder that shows an animated pulsing shape while content is loading.
/// </summary>
[HtmlTargetElement("sa-skeleton")]
public class SkeletonTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "skeleton");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-skeleton", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
