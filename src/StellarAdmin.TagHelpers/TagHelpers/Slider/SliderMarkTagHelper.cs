using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A mark at one value of a <c>&lt;sa-slider&gt;</c>, labelled with its content when it has
///     any.
/// </summary>
[HtmlTargetElement("sa-slider-mark", ParentTag = "sa-slider-marks")]
public class SliderMarkTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The value the mark is placed at, between the slider's minimum and maximum.
    /// </summary>
    [HtmlAttributeName("value")]
    public int Value { get; set; }

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
