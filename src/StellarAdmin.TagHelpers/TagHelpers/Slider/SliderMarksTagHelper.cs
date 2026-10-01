using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The marks along a <c>&lt;sa-slider&gt;</c>. Contains authored <c>&lt;sa-slider-mark&gt;</c>
///     elements, or generates a mark every <see cref="Interval" /> when it has none.
/// </summary>
[HtmlTargetElement("sa-slider-marks", ParentTag = "sa-slider")]
public class SliderMarksTagHelper : StellarAdminTagHelperBase
{
    private const int MaxGeneratedMarks = 200;

    /// <summary>
    ///     The distance between generated marks, starting from the slider's minimum. A mark is
    ///     always generated at the maximum.
    /// </summary>
    /// <remarks>
    ///     Defaults to the slider's <c>step</c>.
    /// </remarks>
    [HtmlAttributeName("interval")]
    public int? Interval { get; set; }

    /// <summary>
    ///     Which generated marks show their formatted value.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SliderMarkLabels.None" />.
    /// </remarks>
    [HtmlAttributeName("labels")]
    public SliderMarkLabels? Labels { get; set; }

    /// <summary>
    ///     Whether each mark draws a tick on the track.
    /// </summary>
    /// <remarks>
    ///     Defaults to <c>true</c>.
    /// </remarks>
    [HtmlAttributeName("show-ticks")]
    public bool? ShowTicks { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slider =
            GetContext<SliderContext>(context)
            ?? throw new InvalidOperationException(
                "<sa-slider-marks> must be placed inside <sa-slider>."
            );
        var effectiveShowTicks = ShowTicks ?? true;
        var effectiveLabels = Labels ?? SliderMarkLabels.None;

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "slider-marks");
        output.Attributes.SetAttribute(
            "data-orientation",
            slider.Orientation.GetDataAttributeText()
        );
        // The thumbs already expose every value, so the marks would only be read as stray text
        output.Attributes.SetAttribute("aria-hidden", "true");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-slider-marks",
                slider.ClassNames?.Marks,
                output.GetUserSuppliedClass()
            )
        );

        SetContext(context, new SliderMarksContext { ShowTicks = effectiveShowTicks });

        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            output.Content.SetHtmlContent(childContent);
            return;
        }

        foreach (var value in GenerateValues(slider, effectiveShowTicks, effectiveLabels))
        {
            var showLabel =
                effectiveLabels == SliderMarkLabels.All
                || effectiveLabels == SliderMarkLabels.Ends
                    && (value == slider.Min || value == slider.Max);
            output.Content.AppendHtml(
                SliderMarkRenderer.Build(
                    slider,
                    effectiveShowTicks,
                    value,
                    showLabel ? slider.FormatValue(value) : null
                )
            );
        }
    }

    private List<int> GenerateValues(SliderContext slider, bool showTicks, SliderMarkLabels labels)
    {
        // Without a tick, an unlabelled mark would render nothing
        if (!showTicks && labels != SliderMarkLabels.All)
        {
            return labels == SliderMarkLabels.Ends ? [slider.Min, slider.Max] : [];
        }

        var interval = Interval ?? slider.Step;
        if (interval <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Interval), "Interval must be positive");
        }

        // Every interval from the minimum, plus the maximum when the intervals miss it
        var count = (slider.Max - slider.Min + interval - 1) / interval + 1;
        if (count > MaxGeneratedMarks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Interval),
                $"Interval would generate more than {MaxGeneratedMarks} marks"
            );
        }

        var values = new List<int>();
        for (var value = slider.Min; value < slider.Max; value += interval)
        {
            values.Add(value);
        }
        values.Add(slider.Max);

        return values;
    }
}
