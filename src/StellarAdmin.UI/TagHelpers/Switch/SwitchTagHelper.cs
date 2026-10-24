using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a control that switches between on and off.
/// </summary>
/// <remarks>
///     The switch is a native checkbox, so it posts and binds like any other checkbox. It works
///     without JavaScript.
/// </remarks>
[HtmlTargetElement("sa-switch", TagStructure = TagStructure.WithoutEndTag)]
public sealed class SwitchTagHelper : FieldInputBaseTagHelper<SwitchClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SwitchTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that renders the bound checkbox and the field's label and messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public SwitchTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the switch posts with.
    /// </summary>
    /// <remarks>
    ///     Use it when the switch isn't inside the form.
    /// </remarks>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets the size of the switch.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="SwitchSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public SwitchSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the value that the switch posts when it is on.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    protected override Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveSize = Size ?? SwitchSize.Default;

        /*
         * Render the native checkbox that acts as the switch track. A switch is a
         * checkbox under the hood so the value posts back and model-binds like any
         * other checkbox, and the on/off visuals are driven purely by CSS (:checked) — no
         * JavaScript required.
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
            // A switch always binds a bool, so the framework input tag helper infers a
            // checkbox (and emits the hidden companion field so an unchecked switch posts
            // "false"). We deliberately leave InputTypeName unset: setting it would make
            // the framework copy a "type" attribute that <sa-switch> never carries.
            var inputTagHelper = new FrameworkInputTagHelper(_htmlGenerator)
            {
                For = For,
                FormName = FormName,
                Value = Value,
                Name = Name,
                ViewContext = ViewContext,
            };
            inputTagHelper.Process(context, inputOutput);
        }

        inputOutput.Attributes.SetAttribute("type", "checkbox");
        inputOutput.Attributes.SetAttribute("role", "switch");

        if (!inputOutput.Attributes.ContainsName("data-slot"))
        {
            inputOutput.Attributes.SetAttribute("data-slot", "switch-input");
        }

        string?[] classNames = ["sa-switch"];

        inputOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                classNames
                    .Union([inputOutput.GetUserSuppliedClass(), output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );

        ApplyFieldAttributes(context, inputOutput.Attributes, FieldLayout.ControlFirst);
        ReadOnlyHandlers.ApplyToCheckbox(inputOutput.Attributes);

        output.Content.AppendHtml(inputOutput);

        // Turn the host element into the switch wrapper, then overlay the thumb.
        output.Attributes.Clear();
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "switch");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-switch-wrapper", ClassNames?.Control)
        );

        var thumb = new TagBuilder("span");
        thumb.Attributes.Add("data-slot", "switch-thumb");
        thumb.Attributes.Add("class", "sa-switch-thumb");
        output.Content.AppendHtml(thumb);

        return Task.FromResult(FieldLayout.ControlFirst);
    }
}
