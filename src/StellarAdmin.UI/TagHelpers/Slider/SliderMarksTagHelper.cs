using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders marks along a slider.
/// </summary>
/// <remarks>
///     Add <c>&lt;sa-slider-mark&gt;</c> elements to place marks yourself. Without them, a mark is
///     generated at every <see cref="Interval" />. Place the marks inside a
///     <c>&lt;sa-slider&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-slider-marks", ParentTag = "sa-slider")]
public class SliderMarksTagHelper : StellarAdminTagHelperBase
{
    private const int MaxGeneratedMarks = 200;

    /// <summary>
    ///     Gets or sets the distance between generated marks.
    /// </summary>
    /// <value>
    ///     The distance in slider values, which must be positive. The default is the slider's step.
    /// </value>
    /// <remarks>
    ///     Marks start at the slider's minimum, and a mark is always generated at the maximum. An
    ///     interval can generate at most 200 marks.
    /// </remarks>
    [HtmlAttributeName("interval")]
    public double? Interval { get; set; }

    /// <summary>
    ///     Gets or sets which generated marks show their value.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies which marks show their value. The default
    ///     is <see cref="SliderMarkLabels.None" />.
    /// </value>
    /// <remarks>
    ///     Values are formatted with the slider's <c>value-format</c>.
    /// </remarks>
    [HtmlAttributeName("labels")]
    public SliderMarkLabels? Labels { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether each mark draws a tick on the track.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if each mark draws a tick; otherwise, <see langword="false" />.
    ///     The default is <see langword="true" />.
    /// </value>
    [HtmlAttributeName("show-ticks")]
    public bool? ShowTicks { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <see cref="Interval" /> isn't positive, or it would generate more than 200 marks.
    /// </exception>
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

    private List<double> GenerateValues(
        SliderContext slider,
        bool showTicks,
        SliderMarkLabels labels
    )
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
        var count = Math.Ceiling(Math.Round((slider.Max - slider.Min) / interval, 9)) + 1;
        if (count > MaxGeneratedMarks)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Interval),
                $"Interval would generate more than {MaxGeneratedMarks} marks"
            );
        }

        var decimals = Math.Max(slider.Decimals, SliderContext.CountDecimals(interval));
        var values = new List<double>();
        for (var index = 0; ; index++)
        {
            // Multiplying rather than accumulating keeps each mark's floating-point error its own
            var value = SliderContext.Round(slider.Min + index * interval, decimals);
            if (value >= slider.Max)
            {
                break;
            }

            values.Add(value);
        }
        values.Add(slider.Max);

        return values;
    }
}
