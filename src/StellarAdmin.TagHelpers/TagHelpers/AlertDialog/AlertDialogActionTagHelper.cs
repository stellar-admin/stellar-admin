using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.TagHelpers;

/// <summary>
///     The confirming button of an alert dialog. Renders a styled submit button with
///     <c>value="confirm"</c> so a wrapping <c>&lt;form method="dialog"&gt;</c> closes the dialog
///     with that <c>returnValue</c> (which the <c>stellarAdmin.alertDialog</c> helper reads as
///     <c>confirmed: true</c>). Override <c>variant</c> for a destructive action.
/// </summary>
[HtmlTargetElement("sa-alert-dialog-action")]
public class AlertDialogActionTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     The visual style of the action button.
    /// </summary>
    /// <remarks>
    ///     Defaults to <see cref="ButtonVariant.Default" />.
    /// </remarks>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? ButtonVariant.Default;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("type", "submit");
        if (!output.Attributes.ContainsName("value"))
        {
            output.Attributes.SetAttribute("value", "confirm");
        }

        // Override Button's data-slot and fold in the alert-dialog-action token
        // *after* the button classes (RenderAttributes folds GetUserSuppliedClass last).
        output.Attributes.SetAttribute("data-slot", "alert-dialog-action");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-action", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, ButtonSize.Default);

        return Task.CompletedTask;
    }
}
