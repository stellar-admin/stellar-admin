using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A form input. Renders a styled <c>&lt;input&gt;</c> for text-like types, and a styled
///     checkbox or radio button (with its indicator) when the type is <c>checkbox</c> or
///     <c>radio</c>. Supports model binding via <c>asp-for</c>.
/// </summary>
[HtmlTargetElement("sa-input", TagStructure = TagStructure.WithoutEndTag)]
public class InputTagHelper : FieldInputBaseTagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    public InputTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager;
    }

    /// <summary>
    ///     The id of the form the input is associated with.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     A composite format string used to format the bound model value.
    /// </summary>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     The <c>type</c> of the HTML input (for example <c>text</c>, <c>email</c>, <c>checkbox</c>,
    ///     or <c>radio</c>).
    /// </summary>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     The value of the input.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    protected override async Task<AutoFieldConfiguration> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        /*
         * Render the select
         */
        var inputOutput = new TagHelperOutput(
            "input",
            new TagHelperAttributeList(output.Attributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagOnly,
        };
        if (For == null)
        {
            if (InputTypeName != null)
            {
                inputOutput.CopyHtmlAttribute("type", context);
            }

            if (Value != null)
            {
                inputOutput.CopyHtmlAttribute("value", context);
            }

            if (FormName != null)
            {
                inputOutput.CopyHtmlAttribute("form", context);
            }
        }
        else
        {
            var inputTagHelper = new FrameworkInputTagHelper(_htmlGenerator)
            {
                For = For,
                Format = Format,
                FormName = FormName,
                InputTypeName = InputTypeName,
                Value = Value,
                Name = Name,
                ViewContext = ViewContext,
            };
            inputTagHelper.Process(context, inputOutput);
        }

        var type = inputOutput.Attributes.TryGetAttribute("type", out var typeAttribute)
            ? typeAttribute.Value?.ToString()
            : null;

        // The framework gives every radio bound to the same property the same id, so their labels
        // all target the first one. Suffix the value to keep each radio and its label paired.
        if (
            For != null
            && type == "radio"
            && Value != null
            && !output.Attributes.ContainsName("id")
            && inputOutput.Attributes["id"]?.Value?.ToString() is { Length: > 0 } radioId
        )
        {
            var perValueId = TagBuilder.CreateSanitizedId($"{radioId}_{Value}", "_");
            inputOutput.Attributes.SetAttribute("id", perValueId);
            LabelForId = perValueId;
        }

        string?[] classNames = type switch
        {
            "checkbox" => ["sa-checkbox", "peer"],
            "radio" => ["sa-radiobutton", "peer"],
            _ => ["sa-input"],
        };

        if (!inputOutput.Attributes.ContainsName("data-slot"))
        {
            inputOutput.Attributes.SetAttribute("data-slot", "input");
        }

        inputOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                classNames
                    .Union([inputOutput.GetUserSuppliedClass(), output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );

        output.Content.AppendHtml(inputOutput);

        output.Attributes.Clear();
        switch (type)
        {
            case "checkbox":
                // Wrap it in a span
                output.TagName = "span";
                output.TagMode = TagMode.StartTagAndEndTag;

                output.Attributes.SetAttribute("class", "sa-input-control-wrapper");

                // Add the span
                var checkboxSpan = new TagBuilder("span");
                checkboxSpan.Attributes.Add("class", JoinCssClasses("sa-checkbox-indicator"));

                // Add the icon
                var checkboxIconOutput = new TagHelperOutput(
                    "svg",
                    [new TagHelperAttribute("class", "sa-checkbox-indicator-icon")],
                    (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );
                var checkboxIconTagHelper = new IconTagHelper(_iconManager) { Name = "check" };
                await checkboxIconTagHelper.ProcessAsync(context, checkboxIconOutput);
                checkboxSpan.InnerHtml.AppendHtml(checkboxIconOutput);

                output.Content.AppendHtml(checkboxSpan);
                break;
            case "radio":
                // Wrap it in a span
                output.TagName = "span";
                output.TagMode = TagMode.StartTagAndEndTag;

                output.Attributes.SetAttribute("class", "sa-input-control-wrapper");

                // Add the span
                var spanTagBuilder = new TagBuilder("span");
                spanTagBuilder.Attributes.Add("class", JoinCssClasses("sa-radiobutton-indicator"));

                // Add the icon
                var iconOutput = new TagHelperOutput(
                    "svg",
                    [
                        new TagHelperAttribute(
                            "class",
                            JoinCssClasses("sa-radiobutton-indicator-icon")
                        ),
                    ],
                    (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );
                var iconTagHelper = new IconTagHelper(_iconManager) { Name = "circle" };
                await iconTagHelper.ProcessAsync(context, iconOutput);
                spanTagBuilder.InnerHtml.AppendHtml(iconOutput);

                output.Content.AppendHtml(spanTagBuilder);
                break;
            default:
                output.TagName = null;
                break;
        }

        return type switch
        {
            // We do not automatically display the error label for radio buttons,
            // since you typically do not want to have the error displayed on each
            // individual radio button. Rather, the error will be displayed for the
            // group of radio buttons as a whole. As such, the developer will need
            // to add the sa-field-error Tag Helper explicitly to their form.
            "radio" when For != null => new AutoFieldConfiguration(
                AutoFieldLayout.HorizontalInputFirst,
                AutoFieldElement.Label | AutoFieldElement.Description
            ),
            "checkbox" or "radio" => new AutoFieldConfiguration(
                AutoFieldLayout.HorizontalInputFirst
            ),
            _ => new AutoFieldConfiguration(AutoFieldLayout.Vertical),
        };
    }
}
