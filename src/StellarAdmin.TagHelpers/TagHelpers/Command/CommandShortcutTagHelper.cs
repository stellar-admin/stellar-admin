using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The keyboard shortcut for a command menu item, aligned to the item's end.
/// </summary>
[HtmlTargetElement("sa-command-shortcut")]
public class CommandShortcutTagHelper : StellarAdminTagHelperBase
{
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "command-shortcut");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-shortcut", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
