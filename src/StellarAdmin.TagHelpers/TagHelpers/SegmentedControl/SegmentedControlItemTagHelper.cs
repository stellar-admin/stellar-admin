using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A labeled radio button within a segmented control.
/// </summary>
[HtmlTargetElement("sa-segmented-control-item")]
public class SegmentedControlItemTagHelper : StellarAdminTagHelperBase
{
    private static readonly object InputIdKey = new();

    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Whether this option is disabled.
    /// </summary>
    public bool? Disabled { get; set; }

    /// <summary>
    ///     The value submitted when this option is selected.
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    ///     The context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public SegmentedControlItemTagHelper(IHtmlGenerator htmlGenerator)
    {
        _htmlGenerator = htmlGenerator;
    }

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
        if (group.For == null && string.Equals(group.Value, Value, StringComparison.Ordinal))
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

        if (!string.IsNullOrEmpty(group.DescribedBy))
        {
            input.Attributes["aria-describedby"] = group.DescribedBy;
        }

        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.RemoveAll("value");
        output.Attributes.RemoveAll("disabled");
        output.Attributes.SetAttribute("for", input.Attributes["id"]);
        output.Attributes.SetAttribute("data-slot", "segmented-control-item");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-segmented-control-item", output.GetUserSuppliedClass())
        );

        var content = await output.GetChildContentAsync();
        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(content);
    }
}
