using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;
using FrameworkInputTagHelper = Microsoft.AspNetCore.Mvc.TagHelpers.InputTagHelper;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a one-time code input with a slot for each character.
/// </summary>
/// <remarks>
///     <para>
///         The whole code posts as one form value. Slots are generated from <c>maxlength</c> and
///         <c>groups</c>, or written by hand with <c>&lt;sa-input-otp-group&gt;</c>,
///         <c>&lt;sa-input-otp-slot&gt;</c> and <c>&lt;sa-input-otp-separator&gt;</c>.
///     </para>
///     <para>
///         Requires <c>stellar-admin.js</c>.
///     </para>
/// </remarks>
[HtmlTargetElement("sa-input-otp")]
public sealed class InputOtpTagHelper : FieldInputBaseTagHelper<InputOtpClassNames>
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InputOtpTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders a bound input, label and error.</param>
    /// <param name="iconOptions">The icon options that supply the separator icon.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public InputOtpTagHelper(IHtmlGenerator htmlGenerator, IOptions<IconOptions> iconOptions)
        : base(htmlGenerator)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets the number of characters in the code.
    /// </summary>
    /// <value>
    ///     The default is the sum of the group sizes, or 6 when no groups are set.
    /// </value>
    [HtmlAttributeName("maxlength")]
    public int? MaxLength { get; set; }

    /// <summary>
    ///     Gets or sets the regular expression that the code must match.
    /// </summary>
    /// <value>
    ///     The default is <c>^\d*$</c>, which accepts digits only.
    /// </value>
    /// <remarks>
    ///     Characters that would stop the code from matching are rejected as they're typed.
    /// </remarks>
    [HtmlAttributeName("pattern")]
    public string? Pattern { get; set; }

    /// <summary>
    ///     Gets or sets the sizes of the groups that the slots are split into.
    /// </summary>
    /// <value>
    ///     A comma-separated list of positive numbers, such as <c>3,3</c>. The default is one group.
    /// </value>
    /// <remarks>
    ///     When <see cref="MaxLength" /> is also set, the sizes must add up to it.
    /// </remarks>
    [HtmlAttributeName("groups")]
    public string? Groups { get; set; }

    /// <summary>
    ///     Gets or sets the kind of on-screen keyboard to show.
    /// </summary>
    /// <value>
    ///     An HTML <c>inputmode</c> value. The default is <c>numeric</c>.
    /// </value>
    [HtmlAttributeName("inputmode")]
    public string? InputMode { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the input is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the input is disabled; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the input is shown as invalid.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the input is shown as invalid; otherwise,
    ///     <see langword="false" />. The default is based on whether the field has an error.
    /// </value>
    [HtmlAttributeName("aria-invalid")]
    public bool? AriaInvalid { get; set; }

    /// <summary>
    ///     Gets or sets the code.
    /// </summary>
    /// <remarks>
    ///     Used when the input isn't bound with <c>asp-for</c>.
    /// </remarks>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the <c>id</c> of the form that the input belongs to.
    /// </summary>
    [HtmlAttributeName("form")]
    public string? FormName { get; set; }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     <see cref="Groups" /> has no sizes or a size that isn't positive, or its sizes don't add
    ///     up to <see cref="MaxLength" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     <see cref="MaxLength" /> isn't positive.
    /// </exception>
    protected override async Task<FieldLayout> RenderInput(
        TagHelperContext context,
        TagHelperOutput output,
        IDictionary<string, object?>? htmlAttributes
    )
    {
        var groupSizes = ResolveGroupSizes(out var maxLength);
        var effectivePattern = string.IsNullOrEmpty(Pattern) ? @"^\d*$" : Pattern;
        var effectiveInputMode = string.IsNullOrEmpty(InputMode) ? "numeric" : InputMode;
        var effectiveDisabled = Disabled ?? false;

        var code = For?.Model?.ToString() ?? Value ?? string.Empty;
        var fieldName = ResolveName();
        var hasError = AriaInvalid ?? (!string.IsNullOrEmpty(Error) || HasModelError(fieldName));
        var userClass = output.GetUserSuppliedClass();

        // The single real input: it holds the whole code and posts it as one value. We overlay it
        // transparently over the slots so a click anywhere focuses it.
        var inputOutput = RenderUnderlyingInput(
            context,
            fieldName,
            maxLength,
            effectiveInputMode,
            hasError
        );

        // The host becomes <sel-input-otp>, the flex container. Drop the name the base copied onto
        // the host: the value posts through the real input, not the host element. The id moves
        // there too, so a label targeting it reaches the focusable control.
        output.Attributes.RemoveAll("name");
        if (For == null && output.Attributes["id"]?.Value is { } hostId)
        {
            inputOutput.Attributes.SetAttribute("id", hostId);
            output.Attributes.RemoveAll("id");
        }

        // The real input is the control a screen reader reaches, so it carries the descriptions.
        if (output.Attributes["aria-describedby"] is { } describedBy)
        {
            inputOutput.Attributes.SetAttribute(describedBy);
            output.Attributes.Remove(describedBy);
        }

        if (AriaInvalid == false)
        {
            inputOutput.Attributes.SetAttribute("aria-invalid", "false");
        }

        ApplyFieldAttributes(context, inputOutput.Attributes, FieldLayout.Stacked);
        output.TagName = "sel-input-otp";
        output.TagMode = TagMode.StartTagAndEndTag;

        // Mark the container with data-input-otp-container and put data-slot="input-otp" on the
        // real input instead.
        output.Attributes.SetAttribute("data-input-otp-container", "true");
        output.Attributes.SetAttribute(
            "maxlength",
            maxLength.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute("data-pattern", effectivePattern);
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-otp", ClassNames?.Control, userClass)
        );
        // Inline container styles: the
        // positioning context for the absolutely-overlaid input, the text-field affordances, and
        // --root-height, which the input's font-size keys off so the transparent text lines up with
        // the slots. The 32px fallback matches the universal size-8 slot; the sel-input-otp web
        // component re-measures and overrides it on hydration for custom slot sizes.
        output.Attributes.SetAttribute(
            "style",
            "position: relative; cursor: text; user-select: none; pointer-events: none; "
                + "--root-height: 32px;"
        );

        // Hand the fake-caret classes to the web component, which builds the caret element on
        // hydration and can't compose classes itself.
        output.Attributes.SetAttribute("data-caret-class", "sa-input-otp-caret");
        output.Attributes.SetAttribute("data-caret-line-class", "sa-input-otp-caret-line");

        // Publish the shared state before rendering children so authored slots can read it.
        SetContext(
            context,
            new InputOtpContext
            {
                ClassNames = ClassNames,
                Code = code,
                MaxLength = maxLength,
                Disabled = effectiveDisabled,
                HasError = hasError,
            }
        );

        var childContent = await output.GetChildContentAsync();
        if (!childContent.IsEmptyOrWhiteSpace)
        {
            // Author composed groups/slots/separators by hand.
            output.Content.AppendHtml(childContent);
        }
        else
        {
            await AppendAutoGeneratedGroupsAsync(context, output, groupSizes, code, hasError);
        }

        // Render the field last, inside an absolute overlay wrapper (matching guilhermerodz), so it
        // covers the slots and receives pointer events ahead of them. The wrapper itself is
        // pointer-events:none; the input re-enables pointer-events:all in its own inline style.
        // The input is 40px wider than the container, so the wrapper clips it, or a container
        // stretched to the field width would widen the page.
        var inputWrapper = new TagBuilder("div");
        inputWrapper.Attributes.Add(
            "style",
            "position:absolute;inset:0;pointer-events:none;overflow:hidden"
        );
        inputWrapper.InnerHtml.AppendHtml(inputOutput);
        output.Content.AppendHtml(inputWrapper);

        return FieldLayout.Stacked;
    }

    private TagHelperOutput RenderUnderlyingInput(
        TagHelperContext context,
        string? fieldName,
        int maxLength,
        string inputMode,
        bool hasError
    )
    {
        var inputOutput = new TagHelperOutput(
            "input",
            [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        )
        {
            TagMode = TagMode.StartTagOnly,
        };

        if (For == null)
        {
            inputOutput.Attributes.SetAttribute("type", "text");
            if (Value != null)
            {
                inputOutput.Attributes.SetAttribute("value", Value);
            }
            if (fieldName != null)
            {
                inputOutput.Attributes.SetAttribute("name", fieldName);
            }
            if (FormName != null)
            {
                inputOutput.Attributes.SetAttribute("form", FormName);
            }
        }
        else
        {
            // Defer to the framework input helper for the name, the bound value, and the
            // unobtrusive-validation attributes (incl. the .input-validation-error class on error).
            // Leave InputTypeName unset: the framework infers "text" from the string model.
            // Setting it would make the helper copy a non-existent "type" attribute off the host.
            var frameworkInput = new FrameworkInputTagHelper(_htmlGenerator)
            {
                For = For,
                FormName = FormName,
                Value = Value,
                Name = Name,
                ViewContext = ViewContext,
            };
            frameworkInput.Process(context, inputOutput);
            inputOutput.Attributes.SetAttribute("type", "text");
        }

        inputOutput.Attributes.SetAttribute("data-slot", "input-otp");
        inputOutput.Attributes.SetAttribute(
            "maxlength",
            maxLength.ToString(CultureInfo.InvariantCulture)
        );
        inputOutput.Attributes.SetAttribute("inputmode", inputMode);
        inputOutput.Attributes.SetAttribute("autocomplete", "one-time-code");
        inputOutput.Attributes.SetAttribute("autocorrect", "off");
        inputOutput.Attributes.SetAttribute("spellcheck", "false");
        // Carry the theme token (cn-input-otp-input) + the cross-theme disabled cursor, folding in
        // any class the framework input helper added (e.g. .input-validation-error on error).
        inputOutput.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-input-otp-input", inputOutput.Attributes["class"]?.Value?.ToString())
        );
        // Drive the group's has-aria-invalid styling from the server-rendered validation state.
        if (hasError)
        {
            inputOutput.Attributes.SetAttribute("aria-invalid", "true");
        }
        if (Disabled == true)
        {
            inputOutput.Attributes.SetAttribute("disabled", "disabled");
        }
        // Inline styles, so the overlay does not depend on any stylesheet. The text and caret are transparent because the slots draw
        // the value and the fake caret; the field exists only to capture native input. The
        // ::selection highlight (which inline styles can't reach) is hidden by the web component.
        inputOutput.Attributes.SetAttribute(
            "style",
            "position: absolute; inset: 0px; width: calc(100% + 40px); height: 100%; display: flex; "
                + "text-align: left; opacity: 1; color: transparent; pointer-events: all; "
                + "background: transparent; caret-color: transparent; border: 0px solid transparent; "
                + "outline: transparent solid 0px; box-shadow: none; line-height: 1; "
                + "letter-spacing: -0.5em; font-size: var(--root-height); font-family: monospace; "
                + "font-variant-numeric: tabular-nums; clip-path: inset(0px 40px 0px 0px);"
        );

        return inputOutput;
    }

    private async Task AppendAutoGeneratedGroupsAsync(
        TagHelperContext context,
        TagHelperOutput output,
        IReadOnlyList<int> groupSizes,
        string code,
        bool hasError
    )
    {
        var index = 0;
        for (var groupIndex = 0; groupIndex < groupSizes.Count; groupIndex++)
        {
            var group = new TagBuilder("div");
            group.Attributes.Add("data-slot", "input-otp-group");
            group.Attributes.Add("class", InputOtpRenderer.GroupClass(ClassNames?.Group));

            for (var slot = 0; slot < groupSizes[groupIndex]; slot++)
            {
                var character = index < code.Length ? code[index].ToString() : null;
                group.InnerHtml.AppendHtml(
                    InputOtpRenderer.BuildSlot(index, character, hasError, ClassNames?.Slot)
                );
                index++;
            }

            output.Content.AppendHtml(group);

            if (groupIndex < groupSizes.Count - 1)
            {
                var separator = new TagHelperOutput(
                    "div",
                    [],
                    (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
                )
                {
                    TagMode = TagMode.StartTagAndEndTag,
                };
                separator.Attributes.SetAttribute("data-slot", "input-otp-separator");
                separator.Attributes.SetAttribute("role", "separator");
                separator.Attributes.SetAttribute(
                    "class",
                    InputOtpRenderer.SeparatorClass(ClassNames?.Separator)
                );
                await InputOtpRenderer.RenderDefaultSeparatorContentAsync(
                    separator.Content,
                    context,
                    _iconOptions
                );
                output.Content.AppendHtml(separator);
            }
        }
    }

    /// <summary>
    ///     Resolves the per-group slot counts and the total <paramref name="maxLength" />: from
    ///     <see cref="Groups" /> when set, otherwise a single group of <see cref="MaxLength" />
    ///     (defaulting to 6).
    /// </summary>
    private IReadOnlyList<int> ResolveGroupSizes(out int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(Groups))
        {
            var sizes = Groups
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(size => int.Parse(size, CultureInfo.InvariantCulture))
                .ToList();

            if (sizes.Count == 0 || sizes.Any(size => size <= 0))
            {
                throw new ArgumentException(
                    "The 'groups' attribute must be a comma-separated list of positive integers.",
                    nameof(Groups)
                );
            }

            var sum = sizes.Sum();
            if (MaxLength.HasValue && MaxLength.Value != sum)
            {
                throw new ArgumentException(
                    "The 'groups' sizes must sum to 'maxlength'.",
                    nameof(Groups)
                );
            }

            maxLength = sum;
            return sizes;
        }

        maxLength = MaxLength ?? 6;
        if (maxLength <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaxLength),
                "Max length must be positive."
            );
        }

        return [maxLength];
    }

    /// <summary>
    ///     Resolves the <c>name</c> applied to the posted input: the explicit
    ///     <see cref="FieldInputBaseTagHelper.Name" /> when supplied, otherwise the fully qualified
    ///     field name from the bound expression (respecting any <c>HtmlFieldPrefix</c>).
    /// </summary>
    private string? ResolveName()
    {
        if (!string.IsNullOrEmpty(Name))
        {
            return Name;
        }

        if (!string.IsNullOrEmpty(For?.Name))
        {
            return ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name);
        }

        return null;
    }

    private bool HasModelError(string? fieldName)
    {
        var modelState = ViewContext.ViewData.ModelState;

        if (
            !string.IsNullOrEmpty(For?.Name)
            && modelState.TryGetValue(For.Name, out var forEntry)
            && forEntry.Errors.Count > 0
        )
        {
            return true;
        }

        return !string.IsNullOrEmpty(fieldName)
            && modelState.TryGetValue(fieldName, out var nameEntry)
            && nameEntry.Errors.Count > 0;
    }
}
