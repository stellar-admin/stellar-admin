using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of radio buttons that posts a single value.
/// </summary>
/// <remarks>
///     Bind it with <c>asp-for</c> to a single value, or set <c>name</c>. Add the options as
///     <c>sa-radio-group-item</c> elements or with <c>asp-items</c>.
/// </remarks>
[HtmlTargetElement("sa-radio-group")]
public sealed class RadioGroupTagHelper : ChoiceGroupTagHelper
{
    /// <summary>
    ///     Gets or sets the order in which the options fill the columns.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the order in which the options fill the
    ///     columns. The default is <see cref="RadioGroupFlow.Across" />.
    /// </value>
    /// <remarks>
    ///     Applies only when the group has more than one column.
    /// </remarks>
    [HtmlAttributeName("flow")]
    public RadioGroupFlow? Flow { get; set; }

    /// <summary>
    ///     Gets or sets the selected value when the group isn't bound with <c>asp-for</c>.
    /// </summary>
    /// <remarks>
    ///     Can't be combined with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets how the options are displayed.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies how the options are displayed. The default is
    ///     <see cref="RadioGroupVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public RadioGroupVariant? Variant { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RadioGroupTagHelper" /> class.
    /// </summary>
    /// <param name="generator">The generator that renders the field and its radio buttons.</param>
    /// <param name="icons">The icon options that the group's inputs use.</param>
    public RadioGroupTagHelper(IHtmlGenerator generator, IOptions<IconOptions> icons)
        : base(generator, icons) { }

    /// <inheritdoc />
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
