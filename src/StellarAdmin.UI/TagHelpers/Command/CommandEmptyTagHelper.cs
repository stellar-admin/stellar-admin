using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the content that a command menu shows when no items match the search.
/// </summary>
[HtmlTargetElement("sa-command-empty")]
public class CommandEmptyTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
