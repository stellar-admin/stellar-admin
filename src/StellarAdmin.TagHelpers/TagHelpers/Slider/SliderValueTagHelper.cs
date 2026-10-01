using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     Displays the current value of a <c>&lt;sa-slider&gt;</c>, rendered as an
///     <c>&lt;output&gt;</c> that the slider fills in the browser and updates as it moves. Inside a
///     field it shows that field's slider; elsewhere, <see cref="For" /> names the slider.
/// </summary>
[HtmlTargetElement("sa-slider-value", TagStructure = TagStructure.WithoutEndTag)]
public class SliderValueTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The <c>id</c> of the slider whose value is shown. Needed only outside a field, or when a
    ///     field holds more than one slider.
    /// </summary>
    [HtmlAttributeName("for")]
    public string? For { get; set; }

    /// <summary>
    ///     The zero-based thumb whose value is shown. When omitted, every value is shown, with a
    ///     range's two values separated by a dash.
    /// </summary>
    [HtmlAttributeName("index")]
    public int? Index { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "output";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "slider-value");
        if (For != null)
        {
            output.Attributes.SetAttribute("for", For);
        }

        if (Index.HasValue)
        {
            output.Attributes.SetAttribute(
                "data-index",
                Index.Value.ToString(CultureInfo.InvariantCulture)
            );
        }

        // The thumb already announces each change, so the output's implicit status role stays quiet
        output.Attributes.SetAttribute("aria-live", "off");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-slider-value", output.GetUserSuppliedClass())
        );
        output.Content.SetContent(string.Empty);
    }
}
