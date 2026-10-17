using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a group of radio buttons as a segmented control.
/// </summary>
/// <remarks>
///     Bind it with <c>asp-for</c> or set <c>name</c>. Add the options as
///     <c>sa-segmented-control-item</c> elements.
/// </remarks>
[HtmlTargetElement("sa-segmented-control")]
public class SegmentedControlTagHelper : FieldInputBaseTagHelper<SegmentedControlClassNames>
{
    /// <summary>
    ///     Gets or sets a value that indicates whether every option is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if every option is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether an option must be selected before the form
    ///     is submitted.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if an option must be selected; otherwise,
    ///     <see langword="false" />. The default is whether the bound property is required.
    /// </value>
    [HtmlAttributeName("required")]
    public bool? Required { get; set; }

    /// <summary>
    ///     Gets or sets the selected value when the control isn't bound with <c>asp-for</c>.
    /// </summary>
    /// <remarks>
    ///     Ignored when bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="SegmentedControlTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the field and its radio buttons.</param>
    public SegmentedControlTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     The control has neither <c>asp-for</c> nor <c>name</c>.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var name =
            For == null ? Name : ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("sa-segmented-control requires asp-for or name.");
        }

        var invalid =
            !string.IsNullOrEmpty(Error)
            || ViewContext.ViewData.ModelState.TryGetValue(name, out var entry)
                && entry.Errors.Count > 0;
        var group = new SegmentedControlContext
        {
            ClassNames = ClassNames,
            Disabled = Disabled ?? false,
            For = For,
            Invalid = invalid,
            Name = name,
            Required = Required ?? For?.Metadata.IsRequired ?? false,
            Value = Value,
        };
        SetContext(context, group);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("role", "radiogroup");
        output.Attributes.SetAttribute("data-slot", "segmented-control");
        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
            && (Label != null || For != null)
        )
        {
            output.Attributes.SetAttribute(
                "aria-label",
                Label ?? For?.Metadata.DisplayName ?? For?.Name
            );
        }

        if (invalid)
        {
            output.Attributes.SetAttribute("aria-invalid", "true");
        }

        ApplyFieldAttributes(context, output.Attributes, FieldLayout.Stacked);

        foreach (var attribute in new[] { "name", "value", "disabled", "required" })
        {
            output.Attributes.RemoveAll(attribute);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                "sa-segmented-control",
                ClassNames?.Control,
                output.GetUserSuppliedClass()
            )
        );

        // Render the options before the field wrapper so its label can target a native radio.
        output.Content.SetHtmlContent(await output.GetChildContentAsync());
        LabelForId = group.FirstInputId;

        return FieldLayout.Stacked;
    }
}
