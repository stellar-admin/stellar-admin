using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the area at the end of a timeline item's title row that holds actions, such as
///     buttons.
/// </summary>
[HtmlTargetElement("sa-timeline-item-actions")]
public sealed class TimelineItemActionsTagHelper : StellarAdminTagHelperBase
{
    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item-actions");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item-actions", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
