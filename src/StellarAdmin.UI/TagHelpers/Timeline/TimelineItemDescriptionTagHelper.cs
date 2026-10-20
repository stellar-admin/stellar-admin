using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the description of a timeline item, shown under its title.
/// </summary>
[HtmlTargetElement("sa-timeline-item-description")]
public sealed class TimelineItemDescriptionTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "p";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item-description");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item-description", output.GetUserSuppliedClass())
        );

        if (GetContext<TimelineItemContext>(context) is { } item)
        {
            item.HasDescription = true;
        }

        return Task.CompletedTask;
    }
}
