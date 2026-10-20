using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an ordered list of events, each with a marker joined to the next by a rail.
/// </summary>
[HtmlTargetElement("sa-timeline")]
public sealed class TimelineTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets what the marker of an item with no media shows.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the marker. The default is
    ///     <see cref="TimelineMarker.Dot" />.
    /// </value>
    [HtmlAttributeName("marker")]
    public TimelineMarker? Marker { get; set; }

    /// <summary>
    ///     Gets or sets how the icons in the items' media are drawn.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="TimelineMediaStyle.Disc" />.
    /// </value>
    [HtmlAttributeName("media-style")]
    public TimelineMediaStyle? MediaStyle { get; set; }

    /// <summary>
    ///     Gets or sets the size of the timeline, which sets the size of its markers and the
    ///     spacing between its items.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="TimelineSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public TimelineSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets where the time of each item is shown.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the placement. The default is
    ///     <see cref="TimelineTimePlacement.End" />.
    /// </value>
    [HtmlAttributeName("time-placement")]
    public TimelineTimePlacement? TimePlacement { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveMarker = Marker ?? TimelineMarker.Dot;
        var effectiveMediaStyle = MediaStyle ?? TimelineMediaStyle.Disc;
        var effectiveSize = Size ?? TimelineSize.Default;
        var effectiveTimePlacement = TimePlacement ?? TimelineTimePlacement.End;

        output.TagName = "ol";
        output.TagMode = TagMode.StartTagAndEndTag;

        // A list styled without numbers loses its list semantics in Safari unless it has the role.
        output.Attributes.SetAttribute("role", "list");
        output.Attributes.SetAttribute("data-slot", "timeline");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute("data-marker", effectiveMarker.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "data-media-style",
            effectiveMediaStyle.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "data-time-placement",
            effectiveTimePlacement.GetDataAttributeText()
        );
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-timeline", output.GetUserSuppliedClass())
        );

        return Task.CompletedTask;
    }
}
