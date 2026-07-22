using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     Groups several <c>&lt;sa-kbd&gt;</c> elements to represent a keyboard shortcut or key sequence.
/// </summary>
[HtmlTargetElement("sa-kbd-group")]
public class KbdGroupTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "kbd";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "kbd-group");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-kbd-group", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
