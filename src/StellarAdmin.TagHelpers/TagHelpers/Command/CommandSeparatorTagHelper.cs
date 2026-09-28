using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A divider between groups in a command menu. It is hidden while a search is active.
/// </summary>
[HtmlTargetElement("sa-command-separator")]
public class CommandSeparatorTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "separator");
        output.Attributes.SetAttribute("data-slot", "command-separator");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-separator", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
