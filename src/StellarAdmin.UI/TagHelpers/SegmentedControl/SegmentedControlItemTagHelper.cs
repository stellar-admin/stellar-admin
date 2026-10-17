using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders an option in a segmented control.
/// </summary>
/// <remarks>
///     Must be placed inside an <c>sa-segmented-control</c> element.
/// </remarks>
[HtmlTargetElement("sa-segmented-control-item")]
public class SegmentedControlItemTagHelper : StellarAdminTagHelperBase
{
    private static readonly object InputIdKey = new();

    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Gets or sets a value that indicates whether the option is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the option is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the option is selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> to select the option, <see langword="false" /> to clear it, or
    ///     <see langword="null" /> to follow the bound or initial value. The default is
    ///     <see langword="null" />.
    /// </value>
    /// <remarks>
    ///     A value posted back to a bound control takes precedence.
    /// </remarks>
    [HtmlAttributeName("selected")]
    public bool? Selected { get; set; }

    /// <summary>
    ///     Gets or sets the value that the control posts when the option is selected.
    /// </summary>
    /// <remarks>
    ///     Required.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SegmentedControlItemTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the radio button of a bound control.</param>
    public SegmentedControlItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The option isn't inside an <c>sa-segmented-control</c> element, or has no <c>value</c>.
    /// </exception>
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var group =
            GetContext<SegmentedControlContext>(context)
            ?? throw new InvalidOperationException(
                "sa-segmented-control-item requires a sa-segmented-control parent."
            );
        if (Value == null)
        {
            throw new InvalidOperationException("sa-segmented-control-item requires a value.");
        }

        var input =
            group.For == null
                ? new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing }
                : _htmlGenerator.GenerateRadioButton(
                    ViewContext,
                    group.For.ModelExplorer,
                    group.For.Name,
                    Value,
                    null,
                    null
                );
        input.Attributes["type"] = "radio";
        input.Attributes["name"] = group.Name;
        input.Attributes["value"] = Value;
        // A request-local sequence also keeps IDs unique inside loops and repeated partials.
        var sequence = (ViewContext.HttpContext.Items[InputIdKey] as int? ?? 0) + 1;
        ViewContext.HttpContext.Items[InputIdKey] = sequence;
        input.Attributes["id"] = $"sa-segmented-control-{GetUniqueId(context)}-{sequence}";
        group.FirstInputId ??= input.Attributes["id"];
        input.Attributes["data-slot"] = "segmented-control-input";
        input.AddCssClass("sa-segmented-control-input");
        // A posted value decides a bound group's selection, as it does without Selected
        var posted =
            group.For != null
            && ViewContext.ViewData.ModelState.TryGetValue(group.Name, out var entry)
            && entry.AttemptedValue != null;
        if (Selected is { } selected && !posted)
        {
            if (selected)
            {
                input.Attributes["checked"] = "checked";
            }
            else
            {
                input.Attributes.Remove("checked");
            }
        }
        else if (group.For == null && string.Equals(group.Value, Value, StringComparison.Ordinal))
        {
            input.Attributes["checked"] = "checked";
        }

        if (group.Disabled || Disabled == true)
        {
            input.Attributes["disabled"] = "disabled";
        }

        if (group.Required)
        {
            input.Attributes["required"] = "required";
        }

        if (group.Invalid)
        {
            input.Attributes["aria-invalid"] = "true";
        }

        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("value");
        output.Attributes.RemoveAll("disabled");
        output.Attributes.SetAttribute("for", input.Attributes["id"]);
        output.Attributes.SetAttribute("data-slot", "segmented-control-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-segmented-control-item",
                group.ClassNames?.Item,
                output.GetUserSuppliedClass()
            )
        );

        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(content);
    }
}
