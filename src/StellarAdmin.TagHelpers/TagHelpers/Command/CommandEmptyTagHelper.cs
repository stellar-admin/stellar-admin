using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Content shown in a command menu when no items match the search.
/// </summary>
[HtmlTargetElement("sa-command-empty")]
public class CommandEmptyTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("role", "presentation");
        output.Attributes.SetAttribute("hidden", "");
        output.Attributes.SetAttribute("data-slot", "command-empty");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-empty", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
