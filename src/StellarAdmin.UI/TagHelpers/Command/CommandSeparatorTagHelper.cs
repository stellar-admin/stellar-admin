using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a divider between groups in a command menu.
/// </summary>
/// <remarks>
///     The divider is hidden during a search that the browser filters.
/// </remarks>
[HtmlTargetElement("sa-command-separator")]
public class CommandSeparatorTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
