using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a popover.
/// </summary>
[HtmlTargetElement("sa-popover-title")]
public sealed class PopoverTitleTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "h2";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "popover-title");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-popover-title", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
