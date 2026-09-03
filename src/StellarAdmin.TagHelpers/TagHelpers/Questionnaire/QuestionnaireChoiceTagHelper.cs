using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using StellarAdmin.TagHelpers.Icons;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     A fixed answer to a questionnaire item, rendered as a native radio button — or a
///     checkbox when the item accepts multiple answers.
/// </summary>
[HtmlTargetElement("sa-questionnaire-choice")]
public class QuestionnaireChoiceTagHelper : StellarAdminTagHelperBase
{
    private readonly IHtmlGenerator _htmlGenerator;
    private readonly IIconManager _iconManager;

    public QuestionnaireChoiceTagHelper(IHtmlGenerator htmlGenerator, IIconManager iconManager)
    {
        _htmlGenerator = htmlGenerator ?? throw new ArgumentNullException(nameof(htmlGenerator));
        _iconManager = iconManager ?? throw new ArgumentNullException(nameof(iconManager));
    }

    /// <summary>
    ///     Whether the choice is selected. Ignored when the choice is bound with <c>asp-for</c>,
    ///     which takes its state from the model.
    /// </summary>
    [HtmlAttributeName("checked")]
    public bool? Checked { get; set; }

    /// <summary>
    ///     Whether the choice can be selected.
    /// </summary>
    [HtmlAttributeName("disabled")]
    public bool Disabled { get; set; }

    /// <summary>
    ///     An expression to be evaluated against the current model.
    /// </summary>
    [HtmlAttributeName("asp-for")]
    public ModelExpression? For { get; set; }

    /// <summary>
    ///     The name the answer posts under. Set automatically when bound with <c>asp-for</c>.
    /// </summary>
    [HtmlAttributeName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     The shortcut key shown beside the choice, which selects it when pressed.
    /// </summary>
    /// <remarks>
    ///     Overrides the key the choices container would assign, and requires
    ///     <c>stellar-admin.js</c>. Use a single character, since the key is matched against one
    ///     key press.
    /// </remarks>
    [HtmlAttributeName("shortcut")]
    public string? Shortcut { get; set; }

    /// <summary>
    ///     The value the answer posts when the choice is selected.
    /// </summary>
    [HtmlAttributeName("value")]
    public string? Value { get; set; }

    /// <summary>
    ///     Gets the <see cref="ViewContext" /> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public required ViewContext ViewContext { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var isMultiple = GetContext<QuestionnaireItemContext>(context)?.Multiple ?? false;
        var type = isMultiple ? "checkbox" : "radio";

        // Only fall back to the container's running sequence when this choice sets no key of its
        // own, so an explicit shortcut never consumes an auto-assigned one. A disabled choice
        // takes no key either, leaving the sequence unbroken over the choices that can be picked.
        var shortcut =
            Shortcut
            ?? (
                Disabled
                    ? null
                    : GetContext<QuestionnaireChoicesContext>(context)?.TakeNextShortcut()
            );

        var childContent = await output.GetChildContentAsync();

        var input = BuildInput(context, type, shortcut);
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
            JoinCssClasses(
                "sa-questionnaire-choice",
                "group/questionnaire-choice",
                output.GetUserSuppliedClass()
            )
        );

        output.Content.SetHtmlContent(input);
        output.Content.AppendHtml(await BuildIndicatorAsync(context, type));
        output.Content.AppendHtml(BuildLabel(inputId, childContent));

        if (!string.IsNullOrEmpty(shortcut))
        {
            output.Content.AppendHtml(BuildShortcut(shortcut));
        }
    }

    private TagBuilder BuildInput(TagHelperContext context, string type, string? shortcut)
    {
        TagBuilder input;

        if (For != null && type == "radio")
        {
            // The framework generator resolves the name, the checked state and the validation
            // class from the model for us.
            input = _htmlGenerator.GenerateRadioButton(
                ViewContext,
                For.ModelExplorer,
                For.Name,
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
                Name
                ?? (
                    For == null
                        ? null
                        : ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(For.Name)
                );

            if (!string.IsNullOrEmpty(name))
            {
                input.Attributes["name"] = name;

                // A checkbox posting into a collection has no framework generator, so the
                // validation class the choice styles off has to be applied by hand.
                if (
                    For != null
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

            if (IsChecked())
            {
                input.Attributes["checked"] = "checked";
            }
        }

        if (Disabled)
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

        var iconTagHelper = new IconTagHelper(_iconManager) { Name = "check" };
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

    private bool IsChecked()
    {
        if (Checked is { } isChecked)
        {
            return isChecked;
        }

        return For?.Model switch
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
