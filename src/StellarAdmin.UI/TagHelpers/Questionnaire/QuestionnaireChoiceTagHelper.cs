using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using StellarAdmin.UI.Icons;

namespace StellarAdmin.UI;

/// <summary>
///     Renders a fixed answer to a questionnaire item.
/// </summary>
/// <remarks>
///     The choice renders as a native radio button, or as a checkbox when the item accepts
///     multiple answers.
/// </remarks>
[HtmlTargetElement("sa-questionnaire-choice")]
public sealed class QuestionnaireChoiceTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IconOptions _iconOptions;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuestionnaireChoiceTagHelper" /> class.
    /// </summary>
    /// <param name="htmlGenerator">The generator that renders the radio button of a bound question.</param>
    /// <param name="iconOptions">The icon options that supply the mark of a selected checkbox.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="htmlGenerator" /> is <see langword="null" />.
    /// </exception>
    public QuestionnaireChoiceTagHelper(
        IHtmlGenerator htmlGenerator,
        IOptions<IconOptions> iconOptions
    )
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconOptions = iconOptions.Value;
    }

    /// <summary>
    ///     Gets or sets a value that indicates whether the choice is selected.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the choice is selected; otherwise, <see langword="false" />.
    ///     The default is <see langword="false" />.
    /// </value>
    /// <remarks>
    ///     Ignored when the question is bound with <c>asp-for</c>, which takes the selected state
    ///     from the model.
    /// </remarks>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Gets or sets a value that indicates whether the choice is disabled.
    /// </summary>
    /// <value>
    ///     <see langword="true" /> if the choice is disabled and can't be selected; otherwise,
    ///     <see langword="false" />. The default is <see langword="false" />.
    /// </value>
    [HtmlAttributeName("disabled")]
    public bool? Disabled { get; set; }

    /// <summary>
    ///     Gets or sets the shortcut key that selects the choice.
    /// </summary>
    /// <value>
    ///     A single character, shown beside the choice. The default is the next key that the
    ///     choices container assigns.
    /// </value>
    /// <remarks>
    ///     Requires <c>stellar-admin.js</c>.
    /// </remarks>
    [HtmlAttributeName("shortcut")]
    public string? Shortcut { get; set; }

    /// <summary>
    ///     Gets or sets the value that the answer posts when the choice is selected.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets or sets the context of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveDisabled = Disabled ?? false;
        var itemContext = GetContext<QuestionnaireItemContext>(context);
        var type = itemContext?.Multiple ?? false ? "checkbox" : "radio";

        // Only fall back to the container's running sequence when this choice sets no key of its
        // own, so an explicit shortcut never consumes an auto-assigned one. A disabled choice
        // takes no key either, leaving the sequence unbroken over the choices that can be picked.
        var shortcut =
            Shortcut
            ?? (
                effectiveDisabled
                    ? null
                    : GetContext<QuestionnaireChoicesContext>(context)?.TakeNextShortcut()
            );

        var childContent = await output.GetChildContentAsync();

        var input = BuildInput(context, itemContext, type, shortcut);
        // BuildInput always mints an id, so the label always has something to target.
        var inputId = input.Attributes["id"]!;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-choice");
        output.Attributes.SetAttribute("data-type", type);

        if (!string.IsNullOrEmpty(shortcut))
        {
            output.Attributes.SetAttribute("data-shortcut", shortcut);
        }

        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-choice", output.GetUserSuppliedClass())
        );

        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(await BuildIndicatorAsync(context, type));
        output.Content.AppendHtml(BuildLabel(inputId, childContent));

        if (!string.IsNullOrEmpty(shortcut))
        {
            output.Content.AppendHtml(BuildShortcut(shortcut));
        }
    }

    private TagBuilder BuildInput(
        TagHelperContext context,
        QuestionnaireItemContext? itemContext,
        string type,
        string? shortcut
    )
    {
        // The question this choice belongs to names the answer; the choice only says which value
        // that answer takes.
        var expression = itemContext?.For;
        TagBuilder input;

        if (expression != null && type == "radio")
        {
            // The framework generator resolves the name, the checked state and the validation
            // class from the model for us.
            input = _htmlGenerator.GenerateRadioButton(
                ViewContext,
                expression.ModelExplorer,
                expression.Name,
                Value,
                null,
                null
            );
        }
        else
        {
            input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
            input.Attributes["type"] = type;

            var name =
                itemContext?.Name
                ?? (
                    expression == null
                        ? null
                        : ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(expression.Name)
                );

            if (!string.IsNullOrEmpty(name))
            {
                input.Attributes["name"] = name;

                // A checkbox posting into a collection has no framework generator, so the
                // validation class the choice styles off has to be applied by hand.
                if (
                    expression != null
                    && ViewContext.ViewData.ModelState.TryGetValue(name, out var entry)
                    && entry.Errors.Count > 0
                )
                {
                    input.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            if (Value != null)
            {
                input.Attributes["value"] = Value;
            }

            if (IsChecked(expression))
            {
                input.Attributes["checked"] = "checked";
            }
        }

        if (Disabled ?? false)
        {
            input.Attributes["disabled"] = "disabled";
        }

        // Every radio bound to one property gets the same generated id, which would point all
        // the labels at the first choice. Suffix the value so each pairs with its own label.
        var idSeed =
            input.Attributes.GetValueOrDefault("id")
            ?? input.Attributes.GetValueOrDefault("name")
            ?? $"sa-{GetUniqueId(context)}";

        input.Attributes["id"] = TagBuilder.CreateSanitizedId(
            string.IsNullOrEmpty(Value) ? idSeed : $"{idSeed}_{Value}",
            "_"
        );

        // The badge is decorative, so the key is announced from the control it operates.
        if (!string.IsNullOrEmpty(shortcut))
        {
            input.Attributes["aria-keyshortcuts"] = shortcut;
        }

        input.Attributes["data-slot"] = "questionnaire-choice-input";
        input.AddCssClass("sa-questionnaire-choice-input");

        return input;
    }

    private async Task<TagBuilder> BuildIndicatorAsync(TagHelperContext context, string type)
    {
        var indicator = new TagBuilder("span");
        indicator.Attributes["aria-hidden"] = "true";
        indicator.Attributes["data-slot"] = "questionnaire-choice-indicator";
        indicator.AddCssClass("sa-questionnaire-choice-indicator");

        // Only the mark this choice can actually show is rendered. BaseUI emits both and hides one
        // in CSS because the type changes at runtime; here the type is known, and emitting both
        // would leave the dot and the check fighting over the same [data-type] guard.
        if (type == "radio")
        {
            var dot = new TagBuilder("span");
            dot.Attributes["data-slot"] = "questionnaire-choice-indicator-dot";
            dot.AddCssClass("sa-questionnaire-choice-indicator-dot");
            indicator.InnerHtml.AppendHtml(dot);

            return indicator;
        }

        var iconOutput = new TagHelperOutput(
            "svg",
            [
                new TagHelperAttribute("class", "sa-questionnaire-choice-indicator-check"),
                new TagHelperAttribute("data-slot", "questionnaire-choice-indicator-check"),
            ],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent())
        );

        var iconTagHelper = new IconTagHelper(_iconOptions)
        {
            Name = _iconOptions.GetSemanticIconName(SemanticIconRole.ChoiceSelected),
        };
        await iconTagHelper.ProcessAsync(context, iconOutput);
        indicator.InnerHtml.AppendHtml(iconOutput);

        return indicator;
    }

    private static TagBuilder BuildLabel(string inputId, TagHelperContent childContent)
    {
        var label = new TagBuilder("label");
        label.Attributes["for"] = inputId;
        label.Attributes["data-slot"] = "questionnaire-choice-label";
        label.AddCssClass("sa-questionnaire-choice-label");
        label.AddCssClass("sa-questionnaire-choice-content");
        label.InnerHtml.AppendHtml(childContent);

        return label;
    }

    private static TagBuilder BuildShortcut(string shortcut)
    {
        var badge = new TagBuilder("span");
        badge.Attributes["aria-hidden"] = "true";
        badge.Attributes["data-slot"] = "questionnaire-choice-shortcut";
        badge.AddCssClass("sa-questionnaire-choice-shortcut");
        badge.AddCssClass("sa-questionnaire-shortcut");
        badge.InnerHtml.Append(shortcut);

        return badge;
    }

    private bool IsChecked(ModelExpression? expression)
    {
        if (Checked is { } isChecked)
        {
            return isChecked;
        }

        return expression?.Model switch
        {
            null => false,
            string text => string.Equals(text, Value, StringComparison.Ordinal),
            IEnumerable values => values
                .Cast<object?>()
                .Any(value => string.Equals(Stringify(value), Value, StringComparison.Ordinal)),
            var model => string.Equals(Stringify(model), Value, StringComparison.Ordinal),
        };
    }

    private static string? Stringify(object? value) =>
        Convert.ToString(value, CultureInfo.InvariantCulture);
}
