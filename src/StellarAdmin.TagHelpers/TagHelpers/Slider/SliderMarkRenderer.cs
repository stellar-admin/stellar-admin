using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Builds the markup shared by generated marks and authored <c>sa-slider-mark</c> elements.
/// </summary>
internal static class SliderMarkRenderer
{
    public static IEnumerable<KeyValuePair<string, string>> GetAttributes(
        SliderContext slider,
        int value,
        string? userClass
    )
    {
        yield return new("data-slot", "slider-mark");
        yield return new("data-orientation", slider.Orientation.GetDataAttributeText());
        yield return new("data-value", value.ToString(CultureInfo.InvariantCulture));
        yield return new("data-state", slider.IsInRange(value) ? "in-range" : "out-of-range");
        if (value == slider.Min || value == slider.Max)
        {
            // Labels at the bounds align to the track's ends rather than overhanging them
            yield return new("data-bound", value == slider.Min ? "min" : "max");
        }

        yield return new(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                "sa-slider-mark",
                slider.ClassNames?.Mark,
                userClass
            )
        );
        yield return new("style", PositionStyle(slider, value));
    }

    public static IHtmlContent RenderContent(
        SliderContext slider,
        bool showTick,
        IHtmlContent? label
    )
    {
        var content = new HtmlContentBuilder();
        if (showTick)
        {
            var tick = new TagBuilder("span");
            tick.Attributes.Add("data-slot", "slider-mark-tick");
            tick.Attributes.Add("class", "sa-slider-mark-tick");
            content.AppendHtml(tick);
        }

        if (label != null)
        {
            var labelElement = new TagBuilder("span");
            labelElement.Attributes.Add("data-slot", "slider-mark-label");
            labelElement.Attributes.Add(
                "class",
                StellarAdminTagHelperBase.JoinCssClasses(
                    "sa-slider-mark-label",
                    slider.ClassNames?.MarkLabel
                )
            );
            labelElement.InnerHtml.AppendHtml(label);
            content.AppendHtml(labelElement);
        }

        return content;
    }

    public static TagBuilder Build(SliderContext slider, bool showTick, int value, string? label)
    {
        var mark = new TagBuilder("span");
        foreach (var (name, attributeValue) in GetAttributes(slider, value, null))
        {
            mark.Attributes.Add(name, attributeValue);
        }

        mark.InnerHtml.AppendHtml(
            RenderContent(
                slider,
                showTick,
                label == null ? null : new HtmlContentBuilder().Append(label)
            )
        );

        return mark;
    }

    public static void EnsureWithinBounds(SliderContext slider, int value, string paramName)
    {
        if (value < slider.Min || value > slider.Max)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                "Each slider mark must be between the slider's Min and Max"
            );
        }
    }

    // A mark sits where its thumb's centre would. Edge-aligned thumbs travel a track shortened by
    // their own size, so the centre is offset by half a thumb. slider.css sets the thumb size as
    // --_slider-thumb (the client's measurement once it has one); 1rem stands in without the CSS.
    private static string PositionStyle(SliderContext slider, int value)
    {
        var fraction = (double)(value - slider.Min) / (slider.Max - slider.Min);
        var side = slider.Orientation == SliderOrientation.Vertical ? "bottom" : "left";
        if (slider.ThumbAlignment == SliderThumbAlignment.Center)
        {
            return $"{side}: {(fraction * 100d).ToString("0.####", CultureInfo.InvariantCulture)}%;";
        }

        var factor = fraction.ToString("0.######", CultureInfo.InvariantCulture);

        return $"{side}: calc({factor} * (100% - var(--_slider-thumb, 1rem)) + var(--_slider-thumb, 1rem) / 2);";
    }
}
