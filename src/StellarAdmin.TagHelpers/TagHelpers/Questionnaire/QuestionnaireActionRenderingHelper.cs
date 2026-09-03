using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

internal static class QuestionnaireActionRenderingHelper
{
    /// Renders one of the questionnaire's navigation buttons. The button styling is shared with
    /// sa-button; the component class only adds the action's fixed column in the actions grid.
    public static async Task RenderAsync(
        TagHelperOutput output,
        string slot,
        string className,
        ButtonVariant variant,
        ButtonSize size,
        string defaultText
    )
    {
        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        // Set before the shared helper runs, which only fills in a slot that is still missing.
        output.Attributes.SetAttribute("data-slot", slot);

        if (!output.Attributes.ContainsName("type"))
        {
            output.Attributes.SetAttribute("type", "submit");
        }

        ButtonRenderingHelper.RenderAttributes(output, variant, size);

        // No data-variant/data-size: sa-button styles from its variant classes, not attributes,
        // and emits neither, so the actions follow the same contract.
        output.Attributes.SetAttribute(
            "class",
            StellarAdminTagHelperBase.JoinCssClasses(
                className,
                output.Attributes["class"]?.Value?.ToString()
            )
        );

        var content = await output.GetChildContentAsync();

        output.Content.SetHtmlContent(
            content.IsEmptyOrWhiteSpace
                ? new DefaultTagHelperContent().Append(defaultText)
                : content
        );
    }
}
