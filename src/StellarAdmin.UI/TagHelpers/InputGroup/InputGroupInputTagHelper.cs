using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI.TagHelpers;

/// <summary>
///     A text input rendered inside an input group, styled to blend into the group so add-ons appear
///     within a single field.
/// </summary>
[HtmlTargetElement("sa-input-group-input")]
public class InputGroupInputTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    /// <summary>
    ///     An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     A composite format string used to format the bound model value.
    /// </summary>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     The id of the form the input is associated with.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     The <c>type</c> of the underlying HTML input (for example <c>text</c> or <c>email</c>).
    /// </summary>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     The value of the input.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public InputGroupInputTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager;
    }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.SetAttribute("data-slot", "input-group-control");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-group-input", output.GetUserSuppliedClass())
        );

        var inputTagHelper = new InputTagHelper(_htmlGenerator, _iconManager)
        {
            ViewContext = ViewContext,
            For = For,
            InputTypeName = InputTypeName,
            Format = Format,
            FormName = FormName,
            Value = Value,
            ShouldRenderField = false,
        };
        await inputTagHelper.ProcessAsync(context, output);
    }
}
