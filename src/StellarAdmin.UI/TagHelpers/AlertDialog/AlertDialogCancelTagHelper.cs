using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI;

/// <summary>
///     Renders the button that cancels an alert dialog.
/// </summary>
/// <remarks>
///     The button submits with the value <c>cancel</c>, unless it sets its own <c>value</c>.
///     Inside a <c>&lt;form method="dialog"&gt;</c>, it closes the dialog with that return
///     value.
/// </remarks>
[HtmlTargetElement("sa-alert-dialog-cancel")]
public class AlertDialogCancelTagHelper : StellarAdminTagHelperBase
{
    /// <summary>
    ///     Gets or sets the visual style of the button.
    /// </summary>
    /// <value>
    ///     One of the enumeration values that specifies the style. The default is
    ///     <see cref="ButtonVariant.Outline" />.
    /// </value>
    [HtmlAttributeName("variant")]
    public ButtonVariant? Variant { get; set; }

    /// <inheritdoc />
    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var effectiveVariant = Variant ?? ButtonVariant.Outline;

        output.TagName = "button";
        output.TagMode = TagMode.StartTagAndEndTag;

        output.Attributes.SetAttribute("type", "submit");
        if (!output.Attributes.ContainsName("value"))
        {
            output.Attributes.SetAttribute("value", "cancel");
        }

        // Override Button's data-slot and fold in the alert-dialog-cancel token
        // *after* the button classes (RenderAttributes folds GetUserSuppliedClass last).
        output.Attributes.SetAttribute("data-slot", "alert-dialog-cancel");
        output.Attributes.SetAttribute(
            "class",
            JoinCssClasses("sa-alert-dialog-cancel", output.GetUserSuppliedClass())
        );

        ButtonRenderingHelper.RenderAttributes(output, effectiveVariant, ButtonSize.Default);

        return Task.CompletedTask;
    }
}
