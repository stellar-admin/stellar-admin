using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an icon or avatar in place of a timeline item's marker.
/// </summary>
[HtmlTargetElement("sa-timeline-item-media")]
public sealed class TimelineItemMediaTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the text that assistive technology announces before the title, such as
    ///     "Failed", for an icon that adds meaning the title doesn't carry.
    /// </summary>
    [HtmlAttributeName("label")]
    public string? Label { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item-media");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item-media", output.GetUserSuppliedClass())
        );

        if (GetContext<TimelineItemContext>(context) is { } item)
        {
            item.HasMedia = true;
        }

        output.Content.AppendHtml(await output.GetChildContentAsync());

        if (!string.IsNullOrWhiteSpace(Label))
        {
            output.Content.AppendHtml("<span class=\"sr-only\">");
            output.Content.Append($"{Label}: ");
            output.Content.AppendHtml("</span>");
        }
    }
}
