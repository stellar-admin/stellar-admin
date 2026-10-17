using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a mark at one value of a slider.
/// </summary>
/// <remarks>
///     The mark's content, if any, is shown as its label. Place the mark inside a
///     <c>&lt;sa-slider-marks&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-slider-mark", ParentTag = "sa-slider-marks")]
public class SliderMarkTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the value that the mark is placed at.
    /// </summary>
    /// <value>
    ///     A value between the slider's minimum and maximum, inclusive.
    /// </value>
    [HtmlAttributeName("value")]
    public double Value { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <see cref="Value" /> isn't between the slider's minimum and maximum.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var slider =
            GetContext<SliderContext>(context)
            ?? throw new InvalidOperationException(
                "<sa-slider-mark> must be placed inside <sa-slider-marks>."
            );
        var showTicks = GetContext<SliderMarksContext>(context)?.ShowTicks ?? true;
        SliderMarkRenderer.EnsureWithinBounds(slider, Value, nameof(Value));

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        foreach (
            var (name, value) in SliderMarkRenderer.GetAttributes(
                slider,
                Value,
                output.GetUserSuppliedClass()
            )
        )
        {
            output.Attributes.SetAttribute(name, value);
        }

        var childContent = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(
            SliderMarkRenderer.RenderContent(
                slider,
                showTicks,
                childContent.IsEmptyOrWhiteSpace ? null : childContent
            )
        );
    }
}
