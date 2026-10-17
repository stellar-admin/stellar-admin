using System.Globalization;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders how far through a questionnaire the reader is.
/// </summary>
/// <remarks>
///     Set <c>current</c> and <c>total</c> to show "Question 1 of 5" and announce it as a progress
///     bar. Content of your own replaces the text. Set <c>aria-valuetext</c> to match it.
/// </remarks>
[HtmlTargetElement("sa-questionnaire-progress")]
public class QuestionnaireProgressTagHelper : StellarAdminTagHelperBase
{
    private const string DefaultLabel = "Questionnaire progress";

    /// <summary>
    ///     Gets or sets the position of the question being shown.
    /// </summary>
    /// <value>
    ///     A position that starts at 1.
    /// </value>
    [HtmlAttributeName("current")]
    public int? Current { get; set; }

    /// <summary>
    ///     Gets or sets the total number of questions.
    /// </summary>
    [HtmlAttributeName("total")]
    public int? Total { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("data-slot", "questionnaire-progress");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-questionnaire-progress", output.GetUserSuppliedClass())
        );

        var content = await output.GetChildContentAsync();

        // Without both counts there is nothing to announce, so the element stays a plain
        // styled container rather than claiming a progressbar role it cannot fill in.
        if (Current is not { } current || Total is not { } total)
        {
            output.Content.SetHtmlContent(content);

            return;
        }

        var text = string.Format(CultureInfo.CurrentCulture, "Question {0} of {1}", current, total);

        output.Attributes.SetAttribute(
            "data-current",
            current.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute("data-total", total.ToString(CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("role", "progressbar");
        output.Attributes.SetAttribute(
            "aria-valuenow",
            current.ToString(CultureInfo.InvariantCulture)
        );
        output.Attributes.SetAttribute("aria-valuemin", "1");
        output.Attributes.SetAttribute(
            "aria-valuemax",
            total.ToString(CultureInfo.InvariantCulture)
        );

        // Content of your own replaces the default wording, so the announced text has to be
        // yours too, or the progress reads back as something the reader cannot see.
        if (!output.Attributes.ContainsName("aria-valuetext"))
        {
            output.Attributes.SetAttribute("aria-valuetext", text);
        }

        if (
            !output.Attributes.ContainsName("aria-label")
            && !output.Attributes.ContainsName("aria-labelledby")
        )
        {
            output.Attributes.SetAttribute("aria-label", DefaultLabel);
        }

        output.Content.SetHtmlContent(
            content.IsEmptyOrWhiteSpace ? new DefaultTagHelperContent().Append(text) : content
        );
    }
}
