using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The title heading of a popover.
/// </summary>
[HtmlTargetElement("sa-popover-title")]
public class PopoverTitleTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-popover-title", "sa-font-heading", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
