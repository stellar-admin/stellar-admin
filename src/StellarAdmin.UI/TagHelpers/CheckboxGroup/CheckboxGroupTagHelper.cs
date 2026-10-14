using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     A group of checkbox options bound to a collection.
/// </summary>
[HtmlTargetElement("sa-checkbox-group")]
public class CheckboxGroupTagHelper : ChoiceGroupTagHelper
{
    /// <summary>
    ///     The order in which the options fill the columns set with <c>columns</c>.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="CheckboxGroupFlow.Across" />.
    /// </remarks>
    public CheckboxGroupFlow? Flow { get; set; }

    /// <summary>
    ///     The initially selected values when not using model binding.
    /// </summary>
    public IEnumerable<string>? Values { get; set; }

    /// <summary>
    ///     The display format of the options.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="CheckboxGroupVariant.Default" />.
    /// </remarks>
    public CheckboxGroupVariant? Variant { get; set; }

    public CheckboxGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator, icons) { }

    protected override Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveVariant = Variant ?? CheckboxGroupVariant.Default;

        return RenderAsync(
            context,
            output,
            true,
            effectiveVariant == CheckboxGroupVariant.ChoiceCard,
            Flow == CheckboxGroupFlow.Down,
            Values
        );
    }
}
