using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The descriptive body text of a popover, shown beneath the title.
/// </summary>
[HtmlTargetElement("sa-popover-description")]
public class PopoverDescriptionTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-popover-description", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
