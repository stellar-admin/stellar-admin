using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A group of checkbox options bound to a collection.
/// </summary>
[HtmlTargetElement("sa-checkbox-group")]
public class CheckboxGroupTagHelper : ChoiceGroupTagHelper
{
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

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? CheckboxGroupVariant.Default;

        return RenderAsync(
            context,
            output,
            true,
            effectiveVariant == CheckboxGroupVariant.ChoiceCard,
            Values
        );
    }
}
