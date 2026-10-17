using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the header of a popover, which usually holds its title and description.
/// </summary>
[HtmlTargetElement("sa-popover-header")]
public sealed class PopoverHeaderTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-header");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-popover-header", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
