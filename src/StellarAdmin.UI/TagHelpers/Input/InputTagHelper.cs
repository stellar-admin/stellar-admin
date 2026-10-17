using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a form input.
/// </summary>
/// <remarks>
///     A <c>checkbox</c> or <c>radio</c> type renders a styled checkbox or radio button with its
///     indicator. Every other type renders a styled <c>&lt;input&gt;</c>.
/// </remarks>
[HtmlTargetElement("sa-input", TagStructure = TagStructure.WithoutEndTag)]
public sealed class InputTagHelper : FieldInputBaseTagHelper<InputClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound input, label and error.</param>
    /// <param name="iconOptions">The icon options that supply the checkbox and radio marks.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public InputTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : this(htmlGenerator, iconOptions.Value) { }

    internal InputTagHelper(IHtmlGenerator htmlGenerator, IconOptions iconOptions)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions;
    }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the input belongs to.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets the composite format string that formats the bound value.
    /// </summary>
    /// <remarks>
    ///     Applies only when the input is bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("asp-format")]
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets the type of the input, such as <c>text</c>, <c>email</c>, <c>checkbox</c> or
    ///     <c>radio</c>.
    /// </summary>
    /// <value>
    ///     The default is the type that suits the bound property, or text when the input isn't bound.
    /// </value>
    [HtmlAttributeName("type")]
    public string? InputTypeName { get; set; }

    /// <summary>
    ///     Gets or sets the value of the input.
    /// </summary>
    /// <remarks>
    ///     A radio button bound with <c>asp-for</c> is selected when its value matches the property.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    protected override async Task<FieldLayout> RenderInput(
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
            "checkbox" => ["sa-checkbox"],
            "radio" => ["sa-radiobutton"],
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
                    .Union([
                        inputOutput.GetUserSuppliedClass(),
                        output.GetUserSuppliedClass(),
                        type is "checkbox" or "radio" ? null : ClassNames?.Control,
                    ])
                    .ToArray()
            )
        );

        var layout = type switch
        {
            // We do not automatically display the error label for radio buttons,
            // since you typically do not want to have the error displayed on each
            // individual radio button. Rather, the error will be displayed for the
            // group of radio buttons as a whole. As such, the developer will need
            // to add the sa-field-error Tag Helper explicitly to their form.
            "radio" when For != null => FieldLayout.ControlFirst.Without(FieldPart.Error),
            "checkbox" or "radio" => FieldLayout.ControlFirst,
            _ => FieldLayout.Stacked,
        };
        ApplyFieldAttributes(context, inputOutput.Attributes, layout);

        output.Content.AppendHtml(inputOutput);

        output.Attributes.Clear();
        switch (type)
        {
            case "checkbox":
                // Wrap it in a span
                output.TagName = "span";
                output.TagMode = TagMode.StartTagAndEndTag;

                output.Attributes.SetAttribute(
                    "class",
                    JoinCssClasses("sa-input-control-wrapper", ClassNames?.Control)
                );

                // Add the span
                var checkboxSpan = new TagBuilder("span");
                checkboxSpan.Attributes.Add("class", "sa-checkbox-indicator");

                // Add the icon
                var checkboxIconOutput = new TagHelperOutput(
                    "svg",
                    [new TagHelperAttribute("class", "sa-checkbox-indicator-icon")],
                    (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );
                var checkboxIconTagHelper = new IconTagHelper(_iconOptions)
                {
                    Name = _iconOptions.GetSemanticIconName(SemanticIconRole.CheckboxSelected),
                };
                await checkboxIconTagHelper.ProcessAsync(context, checkboxIconOutput);
                checkboxSpan.InnerHtml.AppendHtml(checkboxIconOutput);

                output.Content.AppendHtml(checkboxSpan);
                break;
            case "radio":
                // Wrap it in a span
                output.TagName = "span";
                output.TagMode = TagMode.StartTagAndEndTag;

                output.Attributes.SetAttribute(
                    "class",
                    JoinCssClasses("sa-input-control-wrapper", ClassNames?.Control)
                );

                // Add the span
                var spanTagBuilder = new TagBuilder("span");
                spanTagBuilder.Attributes.Add("class", "sa-radiobutton-indicator");

                // Add the icon
                var iconOutput = new TagHelperOutput(
                    "svg",
                    [new TagHelperAttribute("class", "sa-radiobutton-indicator-icon")],
                    (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                );
                var iconTagHelper = new IconTagHelper(_iconOptions)
                {
                    Name = _iconOptions.GetSemanticIconName(SemanticIconRole.RadioSelected),
                };
                await iconTagHelper.ProcessAsync(context, iconOutput);
                spanTagBuilder.InnerHtml.AppendHtml(iconOutput);

                output.Content.AppendHtml(spanTagBuilder);
                break;
            default:
                output.TagName = null;
                break;
        }

        return layout;
    }
}
