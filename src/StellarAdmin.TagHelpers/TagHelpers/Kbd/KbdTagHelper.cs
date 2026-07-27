using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays a single keyboard key or keystroke.
/// </summary>
[HtmlTargetElement("sa-kbd")]
public class KbdTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "kbd";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "kbd");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-kbd", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
