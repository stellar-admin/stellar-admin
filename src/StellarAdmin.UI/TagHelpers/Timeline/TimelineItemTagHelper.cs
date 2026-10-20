using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an event in a timeline: its marker, a title, time and description, and anything
///     else the item holds.
/// </summary>
[HtmlTargetElement("sa-timeline-item")]
public sealed class TimelineItemTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets a value indicating whether the item is the current one, such as today's
    ///     stop on a trip.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to mark the item as current; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     The current item, the items before it and the rails between them are drawn in the
    ///     accent colour. With several current items, the accent runs to the last of them.
    /// </remarks>
    [HtmlAttributeName("current")]
    public bool? Current { get; set; }

    /// <summary>
    ///     Gets or sets the description shown under the title.
    /// </summary>
    /// <remarks>
    ///     Use a <c>&lt;sa-timeline-item-description&gt;</c> child instead for a description
    ///     with markup. The item can't have both.
    /// </remarks>
    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    /// <summary>
    ///     Gets or sets the line drawn from this item to the next one.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the line. The default is
    ///     <see cref="TimelineRail.Solid" />.
    /// </value>
    [HtmlAttributeName("rail")]
    public TimelineRail? Rail { get; set; }

    /// <summary>
    ///     Gets or sets the time of the item, as display text such as "2 Mar, 09:14".
    /// </summary>
    /// <remarks>
    ///     Use a <c>&lt;sa-timeline-item-time&gt;</c> child instead to add a machine-readable
    ///     date and time. The item can't have both.
    /// </remarks>
    [HtmlAttributeName("time")]
    public string? Time { get; set; }

    /// <summary>
    ///     Gets or sets the title of the item.
    /// </summary>
    /// <remarks>
    ///     Use a <c>&lt;sa-timeline-item-title&gt;</c> child instead for a title with markup or
    ///     a muted title. The item can't have both.
    /// </remarks>
    [HtmlAttributeName("title")]
    public string? Title { get; set; }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The item sets <see cref="Title" />, <see cref="Time" /> or <see cref="Description" />
    ///     and also has the matching child part.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "li";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "timeline-item");
        output.Attributes.SetAttribute(
            "data-rail",
            (Rail ?? TimelineRail.Solid).GetDataAttributeText()
        );

        if (Current == true)
        {
            output.Attributes.SetAttribute("data-current", "true");
            output.Attributes.SetAttribute("aria-current", "step");
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline-item", output.GetUserSuppliedClass())
        );

        var item = new TimelineItemContext();
        SetContext(context, item);

        var childContent = await output.GetChildContentAsync();

        ThrowIfSetTwice("title", !string.IsNullOrEmpty(Title), item.HasTitle);
        ThrowIfSetTwice("time", !string.IsNullOrEmpty(Time), item.HasTime);
        ThrowIfSetTwice("description", !string.IsNullOrEmpty(Description), item.HasDescription);

        var content = new HtmlContentBuilder();

        // Without media, the marker is empty and the stylesheet draws the timeline's marker.
        if (!item.HasMedia)
        {
            var marker = new TagBuilder("span");
            marker.AddCssClass("sa-timeline-item-marker");
            marker.Attributes["data-slot"] = "timeline-item-marker";
            marker.Attributes["aria-hidden"] = "true";
            content.AppendHtml(marker);
        }

        if (!string.IsNullOrEmpty(Title))
        {
            content.AppendHtml(RenderPart("div", "timeline-item-title", Title));
        }

        if (!string.IsNullOrEmpty(Time))
        {
            content.AppendHtml(RenderPart("time", "timeline-item-time", Time));
        }

        if (!string.IsNullOrEmpty(Description))
        {
            content.AppendHtml(RenderPart("p", "timeline-item-description", Description));
        }

        content.AppendHtml(childContent);

        output.Content.SetHtmlContent(content);
    }

    private static IHtmlContent RenderPart(string tagName, string slot, string text)
    {
        var part = new TagBuilder(tagName);
        part.AddCssClass($"sa-{slot}");
        part.Attributes["data-slot"] = slot;
        part.InnerHtml.Append(text);

        return part;
    }

    private static void ThrowIfSetTwice(string attributeName, bool attributeIsSet, bool childIsSet)
    {
        if (attributeIsSet && childIsSet)
        {
            throw new InvalidOperationException(
                $"<sa-timeline-item> can't have both the '{attributeName}' attribute and a <sa-timeline-item-{attributeName}> child."
            );
        }
    }
}
