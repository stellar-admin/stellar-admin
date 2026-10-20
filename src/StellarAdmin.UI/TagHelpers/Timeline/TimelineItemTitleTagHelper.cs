using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the title of a timeline item.
/// </summary>
[HtmlTargetElement("sa-timeline-item-title")]
public sealed class TimelineItemTitleTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the title is shown in a muted colour, such as
    ///     for a system event among comments.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to mute the title; otherwise, <see langword="false" />. The
    ///     default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("muted")]
    public bool? Muted { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item-title");

        if (Muted == true)
        {
            output.Attributes.SetAttribute("data-muted", "true");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item-title", output.GetUserSuppliedClass())
        );

        if (GetContext<TimelineItemContext>(context) is { } item)
        {
            item.HasTitle = true;
        }

        return Task.CompletedTask;
    }
}
