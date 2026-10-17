using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the current value of a slider, which updates as the slider moves.
/// </summary>
/// <remarks>
///     Inside a field, it shows the value of the field's slider. Elsewhere, set
///     <see cref="For" />. Requires <c>stellar-admin.js</c>.
/// </remarks>
[HtmlTargetElement("sa-slider-value", TagStructure = TagStructure.WithoutEndTag)]
public class SliderValueTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the <c>id</c> of the slider whose value is shown.
    /// </summary>
    /// <remarks>
    ///     Needed only outside a field, or when a field holds more than one slider.
    /// </remarks>
    [HtmlAttributeName("for")]
    public string? For { get; set; }

    /// <summary>
    ///     Gets or sets the zero-based index of the thumb whose value is shown.
    /// </summary>
    /// <value>
    ///     The index of a thumb. The default shows every value, with the two values of a range
    ///     separated by a dash.
    /// </value>
    [HtmlAttributeName("index")]
    public int? Index { get; set; }

    /// <inheritdoc />
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
