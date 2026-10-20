using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the time of a timeline item as a <c>&lt;time&gt;</c> element.
/// </summary>
/// <remarks>
///     Put the display text, such as "5 min ago", inside it.
/// </remarks>
[HtmlTargetElement("sa-timeline-item-time")]
public sealed class TimelineItemTimeTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the date and time of the item, rendered as the machine-readable
    ///     <c>datetime</c> attribute.
    /// </summary>
    [HtmlAttributeName("datetime")]
    public DateTimeOffset? DateTime { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "time";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item-time");

        if (DateTime is { } dateTime)
        {
            output.Attributes.SetAttribute(
                "datetime",
                dateTime.ToString("yyyy-MM-dd'T'HH:mm:sszzz", CultureInfo.InvariantCulture)
            );
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item-time", output.GetUserSuppliedClass())
        );

        if (GetContext<TimelineItemContext>(context) is { } item)
        {
            item.HasTime = true;
        }

        return Task.CompletedTask;
    }
}
