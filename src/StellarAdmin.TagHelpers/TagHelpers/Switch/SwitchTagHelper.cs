using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A toggle control that switches between on and off states. Backed by a native
///     checkbox so its value model-binds and posts back like any other checkbox, with the
///     visuals driven entirely by CSS.
/// </summary>
[HtmlTargetElement("sa-switch", TagStructure = TagStructure.WithoutEndTag)]
public class SwitchTagHelper : FieldInputBaseTagHelper
{
    private readonly IHtmlGenerator _htmlGenerator;

    public SwitchTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     The id of the form the switch belongs to, mapped to the <c>form</c> attribute.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     The size of the switch.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="SwitchSize.Default" />.
    /// </remarks>
    [HtmlAttributeName("size")]
    public SwitchSize? Size { get; set; }

    /// <summary>
    ///     The value submitted when the switch is on.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    protected override Task<AutoFieldConfiguration> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveSize = Size ?? SwitchSize.Default;

        /*
         * Render the native checkbox that acts as the switch track. A switch is a
         * checkbox under the hood so the value posts back and model-binds like any
         * other checkbox, and the on/off visuals are driven purely by CSS (:checked /
         * peer-checked) — no JavaScript required.
         */
        var inputOutput = new TagHelperOutput(
            "input",
            new TagHelperAttributeList(output.Attributes),
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );
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

        string?[] classNames = ["sa-switch", "peer"];

        inputOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses(
                classNames
                    .Union([inputOutput.GetUserSuppliedClass(), output.GetUserSuppliedClass()])
                    .ToArray()
            )
        );

        output.Content.AppendHtml(inputOutput);

        // Turn the host element into the switch wrapper, then overlay the thumb.
        output.Attributes.Clear();
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "switch");
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-switch-wrapper", "group/switch")
        );

        var thumb = new TagBuilder("span");
        thumb.Attributes.Add("data-slot", "switch-thumb");
        thumb.Attributes.Add("class", JoinCssClasses("sa-switch-thumb"));
        output.Content.AppendHtml(thumb);

        return Task.FromResult(new AutoFieldConfiguration(AutoFieldLayout.HorizontalInputFirst));
    }
}
