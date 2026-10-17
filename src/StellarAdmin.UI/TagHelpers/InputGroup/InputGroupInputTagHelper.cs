using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an input inside an input group.
/// </summary>
/// <remarks>
///     The input blends into the group, so the group and its add-ons look like one field.
/// </remarks>
[HtmlTargetElement("sa-input-group-input")]
public class InputGroupInputTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Gets or sets the model property that the input is bound to.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     Gets or sets the composite format string that formats the bound value.
    /// </summary>
    /// <remarks>
    ///     Applies only when the input is bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the input belongs to.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets the type of the input, such as <c>text</c> or <c>email</c>.
    /// </summary>
    /// <value>
    ///     The default is the type that suits the bound property, or text when the input isn't bound.
    /// </value>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the value of the input.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputGroupInputTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound input.</param>
    /// <param name="iconOptions">The icon options that supply the input's icons.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public InputGroupInputTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "input-group-control");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group-input", output.GetUserSuppliedClass())
        );

        var inputTagHelper = new InputTagHelper(_htmlGenerator, _iconOptions)
        {
            ViewContext = ViewContext,
            For = For,
            InputTypeName = InputTypeName,
            Format = Format,
            FormName = FormName,
            Value = Value,
            WrapInField = false,
        };
        await inputTagHelper.ProcessAsync(context, output);
    }
}
