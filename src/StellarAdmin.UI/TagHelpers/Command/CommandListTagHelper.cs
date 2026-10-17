using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the scrollable list that holds the groups and items of a command menu.
/// </summary>
/// <remarks>
///     When the application supplies the results itself, it replaces the contents of this element.
/// </remarks>
[HtmlTargetElement("sa-command-list")]
public class CommandListTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var commandContext = GetContext<CommandContext>(context);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        if (commandContext is not null && !output.Attributes.ContainsName("id"))
        {
            output.Attributes.SetAttribute("id", commandContext.ListId);
        }

        output.Attributes.SetAttribute("role", "listbox");
        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
        )
        {
            output.Attributes.SetAttribute("aria-label", "Suggestions");
        }

        output.Attributes.SetAttribute("tabindex", "-1");
        output.Attributes.SetAttribute("data-slot", "command-list");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-command-list", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
