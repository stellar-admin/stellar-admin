using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a popover, below its title.
/// </summary>
[HtmlTargetElement("sa-popover-description")]
public class PopoverDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
