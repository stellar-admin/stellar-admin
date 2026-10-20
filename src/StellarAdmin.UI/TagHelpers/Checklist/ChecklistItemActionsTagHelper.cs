using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the area at the end of a checklist item that holds actions, such as buttons, or other trailing content.
/// </summary>
[HtmlTargetElement("sa-checklist-item-actions")]
public sealed class ChecklistItemActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "checklist-item-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-checklist-item-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
