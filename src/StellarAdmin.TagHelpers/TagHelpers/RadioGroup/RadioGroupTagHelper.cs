using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A group of radio options bound to a scalar value.
/// </summary>
[HtmlTargetElement("sa-radio-group")]
public class RadioGroupTagHelper : ChoiceGroupTagHelper
{
    /// <summary>
    ///     The initially selected value when not using model binding.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    ///     The display format of the options.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="RadioGroupVariant.Default" />.
    /// </remarks>
    public RadioGroupVariant? Variant { get; set; }

    public RadioGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator, icons) { }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? RadioGroupVariant.Default;

        return RenderAsync(
            context,
            output,
            false,
            effectiveVariant == RadioGroupVariant.ChoiceCard,
            Value == null ? null : new[] { Value }
        );
    }
}
