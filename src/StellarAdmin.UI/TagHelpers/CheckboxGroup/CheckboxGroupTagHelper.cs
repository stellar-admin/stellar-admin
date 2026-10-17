using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of checkboxes that posts a set of values.
/// </summary>
/// <remarks>
///     Bind it with <c>asp-for</c> to an array or list, or set <c>name</c>. Add the options as
///     <c>sa-checkbox-group-item</c> elements or with <c>asp-items</c>.
/// </remarks>
[HtmlTargetElement("sa-checkbox-group")]
public class CheckboxGroupTagHelper : ChoiceGroupTagHelper
{
    /// <summary>
    ///     Gets or sets the order in which the options fill the columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the order in which the options fill the
    ///     columns. The default is <see cref="CheckboxGroupFlow.Across" />.
    /// </value>
    /// <remarks>
    ///     Applies only when the group has more than one column.
    /// </remarks>
    [HtmlAttributeName("flow")]
    public CheckboxGroupFlow? Flow { get; set; }

    /// <summary>
    ///     Gets or sets the selected values when the group isn't bound with <c>asp-for</c>.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("values")]
    public IEnumerable<string>? Values { get; set; }

    /// <summary>
    ///     Gets or sets how the options are displayed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the options are displayed. The default is
    ///     <see cref="CheckboxGroupVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public CheckboxGroupVariant? Variant { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheckboxGroupTagHelper" /> class.
    /// </summary>
    /// <param name="generator">The generator that renders the field and its checkboxes.</param>
    /// <param name="icons">The icon options that supply the mark of a selected checkbox.</param>
    public CheckboxGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator, icons) { }

    /// <inheritdoc />
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
