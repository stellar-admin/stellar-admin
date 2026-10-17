using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     A group of radio options bound to a scalar value.
/// </summary>
[HtmlTargetElement("sa-radio-group")]
public class RadioGroupTagHelper : ChoiceGroupTagHelper
{
    /// <summary>
    ///     The order in which the options fill the columns set with <c>columns</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="RadioGroupFlow.Across" />.
    /// </remarks>
    [HtmlAttributeName("flow")]
    public RadioGroupFlow? Flow { get; set; }

    /// <summary>
    ///     The initially selected value when not using model binding.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     The display format of the options.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="RadioGroupVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public RadioGroupVariant? Variant { get; set; }

    public RadioGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator, icons) { }

    protected override Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveVariant = Variant ?? RadioGroupVariant.Default;

        return RenderAsync(
            context,
            output,
            false,
            effectiveVariant == RadioGroupVariant.ChoiceCard,
            Flow == RadioGroupFlow.Down,
            Value == null ? null : new[] { Value }
        );
    }
}
