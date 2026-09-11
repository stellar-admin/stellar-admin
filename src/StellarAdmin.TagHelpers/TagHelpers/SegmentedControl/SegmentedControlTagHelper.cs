using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A group of radio buttons styled as a segmented control.
/// </summary>
[HtmlTargetElement("sa-segmented-control")]
public class SegmentedControlTagHelper : FieldInputBaseTagHelper
{
    /// <summary>
    ///     Whether all options are disabled.
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Whether an option must be selected before submitting the form.
    /// </summary>
    public bool? Required { get; set; }

    /// <summary>
    ///     The initially selected value when not using model binding.
    /// </summary>
    public string? Value { get; set; }

    public SegmentedControlTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator) { }

    protected override async Task<AutoFieldConfiguration> RenderInput(
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
            DescribedBy = output.Attributes["aria-describedby"]?.Value?.ToString(),
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

        foreach (var attribute in new[] { "name", "value", "disabled", "required" })
        {
            output.Attributes.RemoveAll(attribute);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-segmented-control", output.GetUserSuppliedClass())
        );

        // Render the options before the field wrapper so its label can target a native radio.
        output.Content.SetHtmlContent(await output.GetChildContentAsync());
        LabelForId = group.FirstInputId;

        return new AutoFieldConfiguration(AutoFieldLayout.Vertical);
    }
}
