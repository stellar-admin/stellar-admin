using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a button that can be pressed on or off.
/// </summary>
/// <remarks>
///     The toggle is a hidden native checkbox, so it posts and binds like any other checkbox. It
///     works without JavaScript.
/// </remarks>
[HtmlTargetElement("sa-toggle")]
public class ToggleTagHelper : FieldInputBaseTagHelper<ToggleClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToggleTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">
    ///     The generator that renders the bound checkbox and the field's label and messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public ToggleTagHelper(IHtmlGenerator htmlGenerator)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
    }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the toggle posts with.
    /// </summary>
    /// <remarks>
    ///     Use it when the toggle isn't inside the form.
    /// </remarks>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <summary>
    ///     Gets or sets the visual style of the toggle.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ToggleVariant.Default" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ToggleVariant? Variant { get; set; }

    /// <summary>
    ///     Gets or sets the size of the toggle.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the size. The default is
    ///     <see cref="ToggleSize.Default" />.
    /// </value>
    [HtmlAttributeName("size")]
    public ToggleSize? Size { get; set; }

    /// <summary>
    ///     Gets or sets the value that the toggle posts when it is pressed.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <inheritdoc />
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var effectiveVariant = Variant ?? ToggleVariant.Default;
        var effectiveSize = Size ?? ToggleSize.Default;

        // Capture the author's content (icon/text) before we repurpose the host element.
        var childContent = await output.GetChildContentAsync();

        /*
         * Render the hidden native checkbox that carries the on/off value. Backing the toggle
         * with a checkbox means the value posts back and model-binds like any other checkbox —
         * no JavaScript required — while the pressed visuals are driven from CSS (has-[:checked])
         * on the wrapping label.
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
            // A bound toggle binds a bool, so the framework input tag helper infers a checkbox
            // (and emits the hidden companion field so an unpressed toggle posts "false"). We
            // leave InputTypeName unset so the framework does not copy a "type" attribute that
            // <sa-toggle> never carries.
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

        if (!inputOutput.Attributes.ContainsName("data-slot"))
        {
            inputOutput.Attributes.SetAttribute("data-slot", "toggle-input");
        }

        // The input is visually hidden but stays focusable, so keyboard focus + Space toggle it
        // and the label reflects its state via the has-* variants.
        inputOutput.Attributes.SetAttribute("class", "sr-only");
        ApplyFieldAttributes(context, inputOutput.Attributes, FieldLayout.Stacked);

        var userClass = output.GetUserSuppliedClass();

        // Turn the host element into the label wrapper around the input + author content.
        output.Attributes.Clear();
        output.TagName = "label";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("data-slot", "toggle");
        output.Attributes.SetAttribute("data-variant", effectiveVariant.GetDataAttributeText());
        output.Attributes.SetAttribute("data-size", effectiveSize.GetDataAttributeText());
        output.Attributes.SetAttribute(
            "class",
            ToggleRenderingHelper.BuildClass(
                effectiveVariant,
                effectiveSize,
                includeGroupItemToken: false,
                JoinCssClasses(ClassNames?.Control, userClass)
            )
        );

        output.Content.AppendHtml(inputOutput);
        output.Content.AppendHtml(childContent);

        return FieldLayout.Stacked;
    }
}
