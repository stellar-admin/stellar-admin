using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a checklist item, shown under its title.
/// </summary>
[HtmlTargetElement("sa-checklist-item-description")]
public sealed class ChecklistItemDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "checklist-item-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-checklist-item-description", output.GetUserSuppliedClass())
        );

        if (GetContext<ChecklistItemContext>(context) is { } item)
        {
            item.HasDescription = true;
        }

        return Task.CompletedTask;
    }
}
