using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the keyboard shortcut of a command menu item, at the item's end.
/// </summary>
[HtmlTargetElement("sa-command-shortcut")]
public class CommandShortcutTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
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
